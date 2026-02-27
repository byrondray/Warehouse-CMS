public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

public class ClientLibrariesViewModel
{
    public string EnvironmentName { get; set; } = string.Empty;
    public bool IsDevelopment { get; set; }
    public bool IsStaging { get; set; }
    public bool IsProduction { get; set; }
    public bool IsTesting { get; set; }
    public ClientJsConfig JsConfig { get; set; } = new();
}

public class ClientJsConfig
{
    public bool UseMinifiedLibraries { get; set; }
    public bool EnableDebugMode { get; set; }
    public string LogLevel { get; set; } = string.Empty;
    public string ErrorReportingEndpoint { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
}

public class ClientErrorReport
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Type { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Timestamp { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(500)]
    public string Source { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string UserId { get; set; } = string.Empty;
}
