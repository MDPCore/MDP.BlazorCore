using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MDP.Registration;
using Microsoft.AspNetCore.Components.Authorization;

namespace MDP.BlazorCore.Maui
{
    internal class AuthenticationStateProvider : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, IDisposable
    {
        // Fields
        private readonly AuthenticationManager _authenticationManager = null;

        private readonly AuthenticationStateManager _authenticationStateManager = null;

        private bool _initialized = false;


        // Constructors
        public AuthenticationStateProvider(AuthenticationManager authenticationManager, AuthenticationStateManager authenticationStateManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticationManager);
            ArgumentNullException.ThrowIfNull(authenticationStateManager);

            #endregion

            // AuthenticationManager
            _authenticationManager = authenticationManager;

            // AuthenticationStateManager
            _authenticationStateManager = authenticationStateManager;
            _authenticationStateManager.PrincipalChanged += this.AuthenticationStateManager_PrincipalChanged;
        }

        public void Dispose()
        {
            // AuthenticationStateManager
            _authenticationStateManager.PrincipalChanged -= this.AuthenticationStateManager_PrincipalChanged;
        }


        // Methods
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Initialized
            if (_initialized == false)
            {
                // Flag
                _initialized = true;

                // Refresh
                await _authenticationManager.RefreshAsync();
            }

            // ClaimsPrincipal
            var claimsPrincipal = await _authenticationStateManager.GetAsync();
            if (claimsPrincipal == null) throw new InvalidOperationException($"{nameof(claimsPrincipal)}=null");

            // Return
            return new AuthenticationState(claimsPrincipal);
        }


        // Handlers
        private void AuthenticationStateManager_PrincipalChanged(ClaimsPrincipal claimsPrincipal)
        {
            #region Contracts

            if (claimsPrincipal == null) throw new ArgumentException($"{nameof(claimsPrincipal)}=null");

            #endregion

            // Notify
            this.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}