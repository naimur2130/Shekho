namespace Shekho.ViewModels
{
    public class AdminUserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsBlocked { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;

        public string? Qualification { get; set; }
        public bool? IsApproved { get; set; }

        public string? CurrentInstitution { get; set; }
        public string? ProfilePicturePath { get; set; }
    }

}
