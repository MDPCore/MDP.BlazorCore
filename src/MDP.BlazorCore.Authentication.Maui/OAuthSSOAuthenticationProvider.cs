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
        public string Name { get; private set; } = "OAuthSSO";


        // Methods
        public async Task LoginAsync()
        {
            // AuthHandler
            using(var authHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // AuthenticateToken
                var authenticateToken = await authHandler.AuthenticateAsync();
                if (string.IsNullOrEmpty(authenticateToken) == true) throw new InvalidOperationException($"{nameof(authenticateToken)}=null");

                // AccessToken
                var accessToken = await authHandler.GetAccessTokenAsync(authenticateToken);
                if (string.IsNullOrEmpty(accessToken) == true) throw new InvalidOperationException($"{nameof(accessToken)}=null");

                // ClaimsIdentity
                var claimsIdentity = await authHandler.GetUserInformationAsync(authenticateToken);
                if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");
                await _authenticationStateManager.SignInAsync(new ClaimsPrincipal(claimsIdentity));
            }
        }

        public async Task LogoutAsync()
        {
            // AuthHandler
            using (var authHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // AuthenticateToken

                // AccessToken

                // ClaimsIdentity
                await _authenticationStateManager.SignOutAsync();
            }
        }
    }
}
