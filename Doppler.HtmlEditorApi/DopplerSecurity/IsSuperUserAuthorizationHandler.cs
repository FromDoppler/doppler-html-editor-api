using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Doppler.HtmlEditorApi.DopplerSecurity
{
    public partial class IsSuperUserAuthorizationHandler : AuthorizationHandler<DopplerAuthorizationRequirement>
    {
        [LoggerMessage(0, LogLevel.Debug, "The token hasn't super user permissions.")]
        partial void LogUserHasNotSuperUserPermissions();

        [LoggerMessage(1, LogLevel.Debug, "The token super user permissions is false.")]
        partial void LogTokenSuperUserPermissionsIsFalse();

        private readonly ILogger<IsSuperUserAuthorizationHandler> _logger;

        public IsSuperUserAuthorizationHandler(ILogger<IsSuperUserAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DopplerAuthorizationRequirement requirement)
        {
            if (requirement.AllowSuperUser && IsSuperUser(context))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool IsSuperUser(AuthorizationHandlerContext context)
        {
            if (!context.User.HasClaim(c => c.Type.Equals(DopplerSecurityDefaults.SUPERUSER_JWT_KEY, StringComparison.Ordinal)))
            {
                LogUserHasNotSuperUserPermissions();
                return false;
            }

            var isSuperUser = bool.Parse(context.User.FindFirst(c => c.Type.Equals(DopplerSecurityDefaults.SUPERUSER_JWT_KEY, StringComparison.Ordinal)).Value);
            if (isSuperUser)
            {
                return true;
            }

            LogTokenSuperUserPermissionsIsFalse();
            return false;
        }
    }
}
