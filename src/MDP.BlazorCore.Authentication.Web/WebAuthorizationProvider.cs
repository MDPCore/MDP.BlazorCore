using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Web
{
    public class WebAuthorizationProvider : IAuthorizationProvider
    {
        // Fields
        private readonly IOptionsMonitor<AuthenticationOptions> _authenticationOptionsMonitor = null;

        private readonly IOptionsMonitor<CookieAuthenticationOptions> _cookieAuthenticationOptionsMonitor = null;

        private readonly NavigationManager _navigationManager = null;

        private readonly PathString _loginPath = null;


        // Constructors
        public WebAuthorizationProvider(IOptionsMonitor<AuthenticationOptions> authenticationOptionsMonitor, IOptionsMonitor<CookieAuthenticationOptions> cookieAuthenticationOptionsMonitor, NavigationManager navigationManager)
        {
            #region Contracts

            if (authenticationOptionsMonitor == null) throw new ArgumentException($"{nameof(authenticationOptionsMonitor)}=null");
            if (cookieAuthenticationOptionsMonitor == null) throw new ArgumentException($"{nameof(cookieAuthenticationOptionsMonitor)}=null");
            if (navigationManager == null) throw new ArgumentException($"{nameof(navigationManager)}=null");

            #endregion

            // Default
            _authenticationOptionsMonitor = authenticationOptionsMonitor;
            _cookieAuthenticationOptionsMonitor = cookieAuthenticationOptionsMonitor;
            _navigationManager = navigationManager;

            // DefaultScheme
            var defaultScheme = authenticationOptionsMonitor.CurrentValue?.DefaultScheme;
            if (string.IsNullOrEmpty(defaultScheme) == true) throw new InvalidOperationException($"{nameof(defaultScheme)}=null");

            // LoginPath
            var loginPath = cookieAuthenticationOptionsMonitor.Get(defaultScheme)?.LoginPath;
            if (loginPath == null) throw new InvalidOperationException($"{nameof(loginPath)}=null");
            if (loginPath.HasValue == false) throw new InvalidOperationException($"{nameof(loginPath)}.HasValue==false");
            _loginPath = loginPath.Value;
        }

        public void Dispose()
        {
            // Nothing

        }


        // Methods
        public Task LoginAsync(string returnUrl = null)
        {
            // Require
            if(string.IsNullOrEmpty(returnUrl)==true) returnUrl = "/";

            // Redirect
            _navigationManager.NavigateTo($"{_loginPath}?returnUrl={Uri.EscapeDataString(returnUrl)}");

            // Return
            return Task.CompletedTask;
        }
    }
}
