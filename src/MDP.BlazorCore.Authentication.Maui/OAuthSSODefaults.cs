﻿using System;
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

        public static readonly string TokenEndpoint = "{0}.sso/token";

        public static readonly string UserInformationEndpoint = "{0}.sso/userinfo";

        public static readonly string CallbackEndpoint = "{0}0.0.0.0/.auth/login/callback";
    }
}