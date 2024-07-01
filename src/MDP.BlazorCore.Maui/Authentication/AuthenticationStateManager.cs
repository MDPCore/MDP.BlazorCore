using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public abstract class AuthenticationStateManager
    {
        // Methods
        public abstract Task SetAsync(ClaimsPrincipal principal);

        public abstract Task<ClaimsPrincipal> GetAsync();

        public abstract Task ClearAsync();


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
