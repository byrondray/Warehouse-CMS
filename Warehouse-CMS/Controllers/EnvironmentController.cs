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
                // Simulate environment-specific error behavior
                if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
                {
                    // In development, we'll throw a more specific exception with details
                    throw new InvalidOperationException(
                        "This is a detailed development-only exception message"
                    );
                }
                else
                {
                    // In staging/production, we'll throw a generic exception
                    throw new Exception("An error occurred");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in the EnvironmentController");

                // Return different error views based on environment
                if (_environment.IsDevelopment() || _environment.IsEnvironment("Testing"))
                {
                    return View(
                        "DevelopmentError",
                        new ErrorViewModel
                        {
                            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                            Exception = ex,
                        }
                    );
                }
                else
                {
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
    }
}
