namespace Shekho.ViewModels
{
    public class AdminUsersPageViewModel
    {
        public List<AdminUserViewModel> Students { get; set; } = new();
        public List<AdminUserViewModel> Instructors { get; set; } = new();
    }

}
