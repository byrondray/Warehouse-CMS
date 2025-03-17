public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public class ClientLibrariesViewModel
{
    public string EnvironmentName { get; set; }
    public bool IsDevelopment { get; set; }
    public bool IsStaging { get; set; }
    public bool IsProduction { get; set; }
    public bool IsTesting { get; set; }
    public ClientJsConfig JsConfig { get; set; }
}

public class ClientJsConfig
{
    public bool UseMinifiedLibraries { get; set; }
    public bool EnableDebugMode { get; set; }
    public string LogLevel { get; set; }
    public string ErrorReportingEndpoint { get; set; }
    public string EnvironmentName { get; set; }
}

public class ClientErrorReport
{
    public string Type { get; set; }
    public string Message { get; set; }
    public string Url { get; set; }
    public string Timestamp { get; set; }
    public string Source { get; set; }
    public string UserId { get; set; }
}
