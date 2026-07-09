namespace EduSphere.Services.Models
{
    public class EmailSettings
    {
        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public bool EnableSSL { get; set; }
    }
}