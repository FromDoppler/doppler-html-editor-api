using Microsoft.AspNetCore.Authorization;

namespace Doppler.HtmlEditorApi.DopplerSecurity
{
    public class DopplerAuthorizationRequirement : IAuthorizationRequirement
    {
        public bool AllowSuperUser { get; init; }
        public bool AllowOwnResource { get; init; }
    }
}
