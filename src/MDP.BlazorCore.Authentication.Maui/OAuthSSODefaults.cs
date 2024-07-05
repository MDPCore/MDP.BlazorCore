using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSODefaults
    {
        // Constants
        public static readonly string AuthenticationScheme = "OAuthSSO";

        public static readonly string AuthorizationEndpoint = "{0}.sso/authorize";

        public static readonly string LogoutEndpoint = "{0}.sso/logout";

        public static readonly string TokenEndpoint = "{0}.sso/token";

        public static readonly string UserInformationEndpoint = "{0}.sso/userinfo";

        public static readonly string LoginCallbackEndpoint = "{0}.auth/login/callback";

        public static readonly string LogoutCallbackEndpoint = "{0}.auth/logout/callback";
    }
}