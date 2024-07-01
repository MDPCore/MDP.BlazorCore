using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Controls.Handlers;
using System;
using System.Threading.Tasks;
using MDP.BlazorCore.Maui;
using System.Security.Claims;
using Microsoft.Extensions.Hosting;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSOAuthenticationProvider : IAuthenticationProvider
    {
        // Fields
        private readonly OAuthSSOOptions _authOptions = null;

        private readonly IHostEnvironment _hostEnvironment = null;

        private readonly AuthenticationStateManager _authenticationStateManager = null;


        // Constructors
        public OAuthSSOAuthenticationProvider(OAuthSSOOptions authOptions, IHostEnvironment hostEnvironment, AuthenticationStateManager authenticationStateManager)
        {
            #region Contracts

            if (authOptions == null) throw new ArgumentException($"{nameof(authOptions)}=null");
            if (hostEnvironment == null) throw new ArgumentException($"{nameof(hostEnvironment)}=null");
            if (authenticationStateManager == null) throw new ArgumentException($"{nameof(authenticationStateManager)}=null");

            #endregion

            // Default
            _authOptions = authOptions;
            _hostEnvironment = hostEnvironment;
            _authenticationStateManager = authenticationStateManager;
        }


        // Properties
        public string AuthenticationScheme { get; private set; } = OAuthSSODefaults.AuthenticationScheme;


        // Methods
        public async Task LoginAsync()
        {
            // AuthHandler
            using(var authenticateHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // Login
                var authenticateToken = await authenticateHandler.LoginAsync();
                if (authenticateToken == null) throw new InvalidOperationException($"{nameof(authenticateToken)}=null");

                // ClaimsIdentity
                var claimsIdentity = await authenticateHandler.GetUserInformationAsync(authenticateToken.AccessToken);
                if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

                // Save
                await _authenticationStateManager.SaveAsync(new ClaimsPrincipal(claimsIdentity));
            }
        }

        public async Task LogoutAsync()
        {
            // AuthHandler
            using (var authHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // Logout
                await authHandler.LogoutAsync();

                // Clear
                await _authenticationStateManager.ClearAsync();
            }
        }
    }
}
