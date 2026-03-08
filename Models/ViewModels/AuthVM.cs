namespace DoAnCS.Models.ViewModels
{
    public class AuthVM // Gộp cả 2 ViewModel LoginVM và RegisterVM vào 1 ViewModel duy nhất để dễ dàng quản lý
    {
        public LoginVM Login { get; set; } = new LoginVM();
        public RegisterVM Register { get; set; } = new RegisterVM();
    }
}