using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Controls.Handlers;
using System;
using System.Threading.Tasks;
using MDP.BlazorCore.Maui;
using System.Security.Claims;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSOAuthenticationProvider : IAuthenticationProvider
    {
        // Fields
        private readonly OAuthSSOHandler _authHandler = null;

        private readonly UserManager _userManager = null;

        private readonly NavigationManager _navigationManager = null;


        // Constructors
        public OAuthSSOAuthenticationProvider(OAuthSSOHandler authHandler, UserManager userManager, NavigationManager navigationManager)
        {
            #region Contracts

            if (authHandler == null) throw new ArgumentException($"{nameof(authHandler)}=null");
            if (userManager == null) throw new ArgumentException($"{nameof(userManager)}=null");
            if (navigationManager == null) throw new ArgumentException($"{nameof(navigationManager)}=null");

            #endregion

            // Default
            _authHandler = authHandler;
            _userManager = userManager;
            _navigationManager = navigationManager;
        }

        public void Dispose()
        {
            // OAuthSSOHandler
            _authHandler.Dispose();
        }


        // Methods
        public async Task LoginAsync(string returnUrl = null)
        {
            // Require
            if (string.IsNullOrEmpty(returnUrl) == true) returnUrl = "/";

            // AuthenticateToken
            var authenticateToken = await _authHandler.AuthenticateAsync();
            if (string.IsNullOrEmpty(authenticateToken) == true) throw new InvalidOperationException($"{nameof(authenticateToken)}=null");

            // AccessToken
            var accessToken = await _authHandler.GetAccessTokenAsync(authenticateToken);
            if (string.IsNullOrEmpty(accessToken) == true) throw new InvalidOperationException($"{nameof(accessToken)}=null");

            // ClaimsIdentity
            var claimsIdentity = await _authHandler.GetUserInformationAsync(authenticateToken);
            if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");
            await _userManager.SignInAsync(new ClaimsPrincipal(claimsIdentity));

            // Redirect
            _navigationManager.NavigateTo(returnUrl);
        }

        public async Task LogoutAsync(string returnUrl = null)
        {
            // Require
            if (string.IsNullOrEmpty(returnUrl) == true) returnUrl = "/";

            // AuthenticateToken
            
            // AccessToken

            // ClaimsIdentity
            await _userManager.SignOutAsync();

            // Redirect
            _navigationManager.NavigateTo(returnUrl);
        }
    }
}
