namespace CallXApi.ViewModels
{
    public class Register
    {
        public string? email { get; set; }
        public string? password { get; set; }
        
    }

    public class RegisterAdmin
    {
        public string? email { get; set; }
        public string? password { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        
    }
}
