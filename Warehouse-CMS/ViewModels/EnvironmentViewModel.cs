namespace Warehouse_CMS.Models
{
    public class EnvironmentViewModel
    {
        public string EnvironmentName { get; set; } = string.Empty;
        public bool IsDevelopment { get; set; }
        public bool IsStaging { get; set; }
        public bool IsProduction { get; set; }
        public bool IsTesting { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public string WebRootPath { get; set; } = string.Empty;
        public ApplicationSettings Settings { get; set; } = new ApplicationSettings();
    }
}
