using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace WebClient.Infrastructure;

public class CustomTelemetryInitializer: TelemetryInitializerBase
{
    private readonly IServiceProvider _serviceProvider;
        private const string CloudRoleName = "web.client.rabbitmq";

        public CustomTelemetryInitializer(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider) :
            base(
                httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnInitializeTelemetry(HttpContext platformContext, RequestTelemetry requestTelemetry,
            ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                telemetry.Context.Cloud.RoleName = CloudRoleName;
            }

            if (platformContext?.User == null)
                return;

            try
            {
                AddAuthenticationDataToTelemetry(platformContext, telemetry);
            }
            catch (Exception e)
            {
                var logger = _serviceProvider.GetService<ILogger<CustomTelemetryInitializer>>();
                logger.LogCritical(e, "Error trying to add authentication data to telemetry!");
            }
        }

        /// <summary>
        /// Add User data to telemetry using it's http context of the request
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="telemetry"></param>
        private static void AddAuthenticationDataToTelemetry(HttpContext httpContext, ITelemetry telemetry)
        {
            // sub is the subject making the request, the user-Id in this case
            var userGuid = httpContext.User.FindFirst("sub")?.Value;

            // client which is making the call
            var clientId = httpContext.User.FindFirst("client_id")?.Value;

            // the hello device which is making the request
            var helloDeviceId = httpContext.User.FindFirst("hello_device_id")?.Value;

            // role of the user 
            var role = httpContext.User.FindFirst("role")?.Value;

            if (userGuid != null)
                telemetry.Context.User.Id = userGuid;

            if (clientId != null && !telemetry.Context.GlobalProperties.TryGetValue("ClientId", out _))
                telemetry.Context.GlobalProperties.TryAdd("ClientId", clientId);

            if (helloDeviceId != null && !telemetry.Context.GlobalProperties.TryGetValue("HelloDeviceId", out _))
                telemetry.Context.GlobalProperties.TryAdd("HelloDeviceId", helloDeviceId);

            if (role != null && !telemetry.Context.GlobalProperties.TryGetValue("Role", out _))
                telemetry.Context.GlobalProperties.TryAdd("Role", role);
        }
}