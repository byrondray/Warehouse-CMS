using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Controllers
{
    public class EnvironmentController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EnvironmentController> _logger;
        private readonly ApplicationSettings _settings;
        private readonly IConfiguration _configuration;

        public EnvironmentController(
            IWebHostEnvironment environment,
            ILogger<EnvironmentController> logger,
            IOptions<ApplicationSettings> settings,
            IConfiguration configuration
        )
        {
            _environment = environment;
            _logger = logger;
            _settings = settings.Value;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Log different levels to demonstrate environment-specific logging
            _logger.LogTrace("This is a trace log message");
            _logger.LogDebug("This is a debug log message");
            _logger.LogInformation("This is an information log message");
            _logger.LogWarning("This is a warning log message");
            _logger.LogError("This is an error log message");

            // Prepare view model with environment information
            var viewModel = new EnvironmentViewModel
            {
                EnvironmentName = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsStaging = _environment.IsStaging(),
                IsProduction = _environment.IsProduction(),
                IsTesting = _environment.IsEnvironment("Testing"),
                ApplicationName = _environment.ApplicationName,
                ContentRootPath = _environment.ContentRootPath,
                WebRootPath = _environment.WebRootPath,
                Settings = _settings,
            };

            return View(viewModel);
        }

        public IActionResult Error()
        {
            // This simulates an error for demonstration purposes
            try
            {
                // Simulate an error (same for all environments)
                throw new Exception("An error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the EnvironmentController");

                // In Development/Testing, let the built-in developer exception page handle it
                if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
                {
                    // This will re-throw the exception, allowing the developer exception page middleware to catch it
                    throw;
                }

                // For other environments (Production/Staging), use our custom error page
                return View(
                    "Error",
                    new ErrorViewModel
                    {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                    }
                );
            }
        }

        public IActionResult ClientLibraries()
        {
            // Create a view model to pass environment-specific configuration
            var viewModel = new ClientLibrariesViewModel
            {
                EnvironmentName = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsStaging = _environment.IsStaging(),
                IsProduction = _environment.IsProduction(),
                IsTesting = _environment.IsEnvironment("Testing"),

                // Environment-specific JavaScript settings
                JsConfig = new ClientJsConfig
                {
                    // In development/testing, use unminified libraries with source maps
                    UseMinifiedLibraries = !(
                        _environment.IsDevelopment() || _environment.IsEnvironment("Testing")
                    ),

                    // Debug mode only in development/testing
                    EnableDebugMode =
                        _environment.IsDevelopment() || _environment.IsEnvironment("Testing"),

                    // Detailed logging only in development/testing
                    LogLevel =
                        _environment.IsDevelopment() || _environment.IsEnvironment("Testing")
                            ? "debug"
                            : (_environment.IsStaging() ? "warn" : "error"),

                    // Error reporting endpoint - could be different per environment
                    ErrorReportingEndpoint =
                        _configuration["ErrorReporting:Endpoint"] ?? "/api/error-reporting",

                    // Environment name to pass to client scripts
                    EnvironmentName = _environment.EnvironmentName,
                },
            };

            return View(viewModel);
        }

        [ApiController]
        [Route("api/error-reporting")]
        public class ErrorReportingController : ControllerBase
        {
            private readonly ILogger<ErrorReportingController> _logger;
            private readonly IWebHostEnvironment _environment;

            public ErrorReportingController(
                ILogger<ErrorReportingController> logger,
                IWebHostEnvironment environment
            )
            {
                _logger = logger;
                _environment = environment;
            }

            [HttpPost]
            public IActionResult ReportError([FromBody] ClientErrorReport errorReport)
            {
                // Don't log as error in development environment to avoid filling logs
                if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
                {
                    _logger.LogInformation(
                        "Client error reported (Development): {ErrorType} - {ErrorMessage}",
                        errorReport.Type,
                        errorReport.Message
                    );
                }
                else
                {
                    // In production, log as an actual error
                    _logger.LogError(
                        "Client error reported: {ErrorType} - {ErrorMessage} - URL: {Url}",
                        errorReport.Type,
                        errorReport.Message,
                        errorReport.Url
                    );

                    // In a real app, you could save this to a database, send to an error tracking service, etc.
                }

                return Ok();
            }
        }

        public IActionResult StatusCode(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            return View(
                "Error",
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
