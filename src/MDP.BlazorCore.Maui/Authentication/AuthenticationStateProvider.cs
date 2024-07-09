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
        private readonly AuthenticationStateManager _authenticationStateManager;


        // Constructors
        public AuthenticationStateProvider(AuthenticationStateManager authenticationStateManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticationStateManager);

            #endregion

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