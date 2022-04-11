using System;
using Loggly;
using Loggly.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Doppler.HtmlEditorApi.Logging
{
    public static class LogglySetup
    {
        private const string DEFAULT_ENDPOINT_HOSTNAME = "logs-01.loggly.com";
        private const int DEFAULT_ENDPOINT_PORT = 443;

        public static IConfiguration ConfigureLoggly(
            this IConfiguration configuration,
            IHostEnvironment hostingEnvironment,
            string appSettingsSection = nameof(LogglyConfig))
        {
            var config = LogglyConfig.Instance;

            // Set default values
            config.Transport.EndpointPort = DEFAULT_ENDPOINT_PORT;

            // Bind values from configuration
            configuration.GetSection(appSettingsSection).Bind(config);

            // Configure convention values if not set in configuration
            config.ApplicationName ??= hostingEnvironment.ApplicationName;
            config.Transport.EndpointHostname ??= DEFAULT_ENDPOINT_HOSTNAME;

            return configuration;
        }
    }
}
