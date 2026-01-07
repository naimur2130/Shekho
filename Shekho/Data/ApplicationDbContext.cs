using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shekho.Models;

namespace Shekho.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Course> Course { get; set; }
        public DbSet<CourseSection> CourseSection { get; set; }
        public DbSet<Lesson> Lesson { get; set; }
        public DbSet<Quiz> Quiz { get; set; }
        public DbSet<CourseCategory> CourseCategory { get; set; }
        public DbSet<CourseSubCategory> CourseSubCategory { get; set; }

    }
}
