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

        private readonly AuthenticateTokenManager _authenticateTokenManager = null;

        private readonly AuthenticationStateManager _authenticationStateManager = null;


        // Constructors
        public OAuthSSOAuthenticationProvider(OAuthSSOOptions authOptions, IHostEnvironment hostEnvironment, AuthenticateTokenManager authenticateTokenManager, AuthenticationStateManager authenticationStateManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authOptions);
            ArgumentNullException.ThrowIfNull(hostEnvironment);
            ArgumentNullException.ThrowIfNull(authenticateTokenManager);
            ArgumentNullException.ThrowIfNull(authenticationStateManager);

            #endregion

            // Default
            _authOptions = authOptions;
            _hostEnvironment = hostEnvironment;
            _authenticateTokenManager = authenticateTokenManager;
            _authenticationStateManager = authenticationStateManager;
        }


        // Properties
        public string AuthenticationScheme { get; private set; } = OAuthSSODefaults.AuthenticationScheme;


        // Methods
        public async Task LoginAsync()
        {
            // AuthenticateHandler
            using (var authenticateHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // AuthenticateToken
                var authenticateToken = await authenticateHandler.LoginAsync();
                if (authenticateToken == null) throw new InvalidOperationException($"{nameof(authenticateToken)}=null");

                // ClaimsIdentity
                var claimsIdentity = await authenticateHandler.GetUserInformationAsync(authenticateToken.AccessToken);
                if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

                // Save
                await _authenticateTokenManager.SetAsync(authenticateToken);
                await _authenticationStateManager.SetAsync(new ClaimsPrincipal(claimsIdentity));
            }
        }

        public async Task LogoutAsync()
        {
            // AuthenticateHandler
            using (var authenticateHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // Logout
                await authenticateHandler.LogoutAsync();

                // Clear
                await _authenticateTokenManager.RemoveAsync();
                await _authenticationStateManager.RemoveAsync();
            }
        }

        public async Task RefreshAsync()
        {
            // AuthenticateHandler
            using (var authenticateHandler = new OAuthSSOHandler(_authOptions, _hostEnvironment))
            {
                // AuthenticateToken
                var authenticateToken = await _authenticateTokenManager.GetAsync();
                if (authenticateToken == null) return;

                // AuthenticateToken.Refresh
                authenticateToken = await authenticateHandler.RefreshAsync(authenticateToken.RefreshToken);
                if (authenticateToken == null) return;

                // ClaimsIdentity.Refresh
                var claimsIdentity = await authenticateHandler.GetUserInformationAsync(authenticateToken.AccessToken);
                if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

                // Save
                await _authenticateTokenManager.SetAsync(authenticateToken);
                await _authenticationStateManager.SetAsync(new ClaimsPrincipal(claimsIdentity));
            }
        }
    }
}
