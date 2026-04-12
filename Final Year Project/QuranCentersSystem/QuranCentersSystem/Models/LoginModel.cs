namespace QuranCentersSystem.Models // تأكد أن الاسم هنا يطابق اسم مشروعك
{
    public class LoginModel
    {
        // هذا المتغير سيستقبل الإيميل الذي يكتبه المستخدم في Flutter
        public string Username { get; set; }

        // هذا المتغير سيستقبل كلمة المرور
        public string Password { get; set; }
    }
}