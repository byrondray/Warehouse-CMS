// app-error-handler.js
(function () {
  // Configuration object that will be set based on environment
  const config = {
    logLevel: 'error', // 'debug', 'info', 'warn', 'error', or 'none'
    reportToServer: true,
    showUserFriendlyMessages: true,
    errorReportingEndpoint: '/api/error-reporting',
    developmentMode: false,
  };

  // Initialize based on current environment
  function initializeErrorHandler() {
    // Check if we're in development mode by looking for a flag
    // This will be injected by the server when rendering the page
    if (
      window.APP_ENVIRONMENT === 'Development' ||
      window.APP_ENVIRONMENT === 'Testing'
    ) {
      config.logLevel = 'debug';
      config.reportToServer = false;
      config.developmentMode = true;
    } else if (window.APP_ENVIRONMENT === 'Staging') {
      config.logLevel = 'warn';
    }

    // Set up global error handler
    window.onerror = function (message, source, lineno, colno, error) {
      handleError({
        type: 'uncaught',
        message: message,
        source: source,
        lineno: lineno,
        colno: colno,
        error: error,
        timestamp: new Date().toISOString(),
      });
      return false; // Let the default handler run as well
    };

    // Handle promise rejections
    window.addEventListener('unhandledrejection', function (event) {
      handleError({
        type: 'promise',
        message: event.reason
          ? event.reason.message || 'Unhandled Promise Rejection'
          : 'Unhandled Promise Rejection',
        error: event.reason,
        timestamp: new Date().toISOString(),
      });
    });

    // Patch fetch to catch errors
    const originalFetch = window.fetch;
    window.fetch = function (...args) {
      return originalFetch.apply(this, args).catch((error) => {
        handleError({
          type: 'fetch',
          message: error.message || 'Fetch Error',
          url: args[0],
          error: error,
          timestamp: new Date().toISOString(),
        });
        throw error; // Re-throw to maintain original behavior
      });
    };

    console.log(`Error handler initialized in ${window.APP_ENVIRONMENT} mode`);
  }

  // Main error handling function
  function handleError(errorInfo) {
    // Log to console based on logLevel
    if (shouldLog(errorInfo)) {
      if (config.developmentMode) {
        console.group('Application Error');
        console.error('Error details:', errorInfo);
        console.groupEnd();
      } else {
        console.error('Application Error:', errorInfo.message);
      }
    }

    // Report to server in production/staging
    if (config.reportToServer && !config.developmentMode) {
      reportErrorToServer(errorInfo);
    }

    // Show user-friendly message if enabled
    if (config.showUserFriendlyMessages && !config.developmentMode) {
      showUserFriendlyMessage(errorInfo);
    }
  }

  // Determine if we should log based on severity and config
  function shouldLog(errorInfo) {
    if (config.logLevel === 'none') return false;
    if (config.logLevel === 'debug') return true;

    // For other log levels, implement severity checking logic
    // This is a simple example - you might want more sophisticated logic
    if (config.logLevel === 'error' && errorInfo.type === 'uncaught')
      return true;
    if (
      config.logLevel === 'warn' &&
      (errorInfo.type === 'uncaught' || errorInfo.type === 'promise')
    )
      return true;

    return config.logLevel === 'info';
  }

  // Send error details to the server
  function reportErrorToServer(errorInfo) {
    // Don't send sensitive error details in production
    const safeErrorInfo = {
      type: errorInfo.type,
      message: errorInfo.message,
      url: window.location.href,
      timestamp: errorInfo.timestamp,
      // Include user info if available
      userId: window.currentUser ? window.currentUser.id : null,
    };

    if (errorInfo.source) {
      safeErrorInfo.source = errorInfo.source;
    }

    // Use sendBeacon for reliability during page unload
    if (navigator.sendBeacon) {
      const blob = new Blob([JSON.stringify(safeErrorInfo)], {
        type: 'application/json',
      });
      navigator.sendBeacon(config.errorReportingEndpoint, blob);
    } else {
      // Fallback to fetch for older browsers
      fetch(config.errorReportingEndpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(safeErrorInfo),
        keepalive: true,
      }).catch((e) => {
        // Silently fail - we don't want to create an infinite loop of error reporting
        console.error('Failed to report error to server:', e);
      });
    }
  }

  // Show a user-friendly error message
  function showUserFriendlyMessage(errorInfo) {
    // Only show for serious errors, not for minor ones
    if (errorInfo.type !== 'uncaught') return;

    // Check if there's an error container already in the page
    let errorContainer = document.getElementById('app-error-container');

    if (!errorContainer) {
      // Create an error container if it doesn't exist
      errorContainer = document.createElement('div');
      errorContainer.id = 'app-error-container';
      errorContainer.style.cssText =
        'position:fixed; bottom:20px; right:20px; background:#f8d7da; ' +
        'color:#721c24; border:1px solid #f5c6cb; padding:15px; border-radius:4px; ' +
        'box-shadow:0 2px 4px rgba(0,0,0,0.2); max-width:350px; z-index:9999;';

      document.body.appendChild(errorContainer);
    }

    // Create the message element
    const messageEl = document.createElement('div');
    messageEl.style.cssText =
      'margin-bottom:10px; padding-bottom:10px; border-bottom:1px solid #f5c6cb;';
    messageEl.innerHTML = `
            <div style="display:flex; justify-content:space-between; margin-bottom:5px;">
                <strong>Something went wrong</strong>
                <button style="background:none; border:none; cursor:pointer; font-size:16px;" 
                  onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
            <div>We encountered an unexpected error. Please try again or refresh the page.</div>
            <div style="font-size:11px; margin-top:5px; opacity:0.8;">Error ID: ${Date.now().toString(
              36
            )}</div>
        `;

    // Add to the container
    errorContainer.appendChild(messageEl);

    // Remove after 8 seconds
    setTimeout(() => {
      if (messageEl.parentNode) {
        messageEl.remove();
      }
      // Remove the container if it's empty
      if (errorContainer.children.length === 0) {
        errorContainer.remove();
      }
    }, 8000);
  }

  // Public API
  window.AppErrorHandler = {
    init: initializeErrorHandler,
    reportError: handleError,
    setConfig: function (newConfig) {
      Object.assign(config, newConfig);
    },
  };

  // Auto-initialize once DOM is loaded
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeErrorHandler);
  } else {
    initializeErrorHandler();
  }
})();
