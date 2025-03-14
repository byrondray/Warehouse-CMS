namespace Warehouse_CMS.Models
{
    public class ApplicationSettings
    {
        public string SiteName { get; set; } = "Default Site Name";
        public string ApiEndpoint { get; set; } = string.Empty;
        public int CacheTimeout { get; set; } = 60;
        public FeatureFlags EnableFeatures { get; set; } = new FeatureFlags();
    }

    public class FeatureFlags
    {
        public bool Analytics { get; set; }
        public bool ExperimentalFeatures { get; set; }
        public bool DetailedLogging { get; set; }
    }
}
