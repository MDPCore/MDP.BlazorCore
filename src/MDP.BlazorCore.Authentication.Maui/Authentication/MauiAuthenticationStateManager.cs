using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class MauiAuthenticationStateManager : AuthenticationStateManager
    {
        // Fields
        private ClaimsPrincipal _currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());


        // Methods
        public override Task<ClaimsPrincipal> AuthenticateAsync()
        {
            // CurrentPrincipal
            var currentPrincipal = _currentPrincipal;

            // Return
            return Task.FromResult(currentPrincipal);
        }

        public override Task SignInAsync(ClaimsPrincipal principal)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(principal);

            #endregion

            // CurrentPrincipal
            var currentPrincipal = principal;
            _currentPrincipal = currentPrincipal;

            // Raise
            this.OnPrincipalChanged(currentPrincipal);

            // Return
            return Task.CompletedTask;
        }

        public override Task SignOutAsync()
        {
            // CurrentPrincipal
            var currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            _currentPrincipal = currentPrincipal;

            // Raise
            this.OnPrincipalChanged(currentPrincipal);

            // Return
            return Task.CompletedTask;
        }
    }
}
