using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication
{
    public abstract class AuthenticationStateManager
    {
        // Methods
        public abstract Task SignInAsync(ClaimsPrincipal principal);

        public abstract Task SignOutAsync();

        public abstract Task<ClaimsPrincipal> AuthenticateAsync();


        // Events
        public event Action<ClaimsPrincipal> PrincipalChanged;
        protected void OnPrincipalChanged(ClaimsPrincipal principal)
        {
            #region Contracts

            if (principal == null) throw new ArgumentException($"{nameof(principal)}=null");

            #endregion

            // Raise
            var handler = this.PrincipalChanged;
            if (handler != null)
            {
                handler(principal);
            }
        }
    }
}
