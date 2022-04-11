using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Doppler.HtmlEditorApi.DopplerSecurity
{
    public class DopplerSecurityOptions
    {
        public IEnumerable<SecurityKey> SigningKeys { get; set; } = new SecurityKey[0];
    }
}
