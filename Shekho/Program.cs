using BeadedShop;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Shekho.Data;
using Shekho.Models;
using Shekho.Services;
using Stripe;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; 
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024; 
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; 
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromDays(1);

    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.Name = "ShekhoAuth";
});


builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())//
{
    var services = scope.ServiceProvider;
    await RoleInitializer.InitializeAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true && context.Request.Path == "/")
    {
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        if (roles.Contains("Admin"))
            context.Response.Redirect("/AdminArea/Admin/Dashboard");
        else if (roles.Contains("Instructor"))
            context.Response.Redirect("/InstructorArea/Instructor/Index");
        else if (roles.Contains("Student"))
            context.Response.Redirect("/StudentArea/Course/Index");
        else
            context.Response.Redirect("/Home/Index");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute( //
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
