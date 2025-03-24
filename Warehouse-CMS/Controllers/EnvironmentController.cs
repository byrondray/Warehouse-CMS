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
            _logger.LogTrace("This is a trace log message");
            _logger.LogDebug("This is a debug log message");
            _logger.LogInformation("This is an information log message");
            _logger.LogWarning("This is a warning log message");
            _logger.LogError("This is an error log message");

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
            try
            {
                throw new Exception("An error occurred");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the EnvironmentController");

                if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
                {
                    throw;
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

        public IActionResult ClientLibraries()
        {
            var viewModel = new ClientLibrariesViewModel
            {
                EnvironmentName = _environment.EnvironmentName,
                IsDevelopment = _environment.IsDevelopment(),
                IsStaging = _environment.IsStaging(),
                IsProduction = _environment.IsProduction(),
                IsTesting = _environment.IsEnvironment("Testing"),

                JsConfig = new ClientJsConfig
                {
                    UseMinifiedLibraries = !(
                        _environment.IsDevelopment() || _environment.IsEnvironment("Testing")
                    ),

                    EnableDebugMode =
                        _environment.IsDevelopment() || _environment.IsEnvironment("Testing"),

                    LogLevel =
                        _environment.IsDevelopment() || _environment.IsEnvironment("Testing")
                            ? "debug"
                            : (_environment.IsStaging() ? "warn" : "error"),

                    ErrorReportingEndpoint =
                        _configuration["ErrorReporting:Endpoint"] ?? "/api/error-reporting",

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
                    _logger.LogError(
                        "Client error reported: {ErrorType} - {ErrorMessage} - URL: {Url}",
                        errorReport.Type,
                        errorReport.Message,
                        errorReport.Url
                    );
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
