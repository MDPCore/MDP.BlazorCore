using MDP.AspNetCore.Authentication;
using MDP.Registration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Web
{
    public class WebAuthenticationStateManager : AuthenticationStateManager
    {
        // Fields
        private readonly IHttpContextAccessor _httpContextAccessor = null;


        // Constructors
        public WebAuthenticationStateManager(IHttpContextAccessor httpContextAccessor)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(httpContextAccessor);

            #endregion

            // Default
            _httpContextAccessor = httpContextAccessor;
        }


        // Methods
        public override Task<ClaimsPrincipal> GetPrincipalAsync()
        {
            // CurrentPrincipal
            var currentPrincipal = _httpContextAccessor.HttpContext.User;
            if (currentPrincipal == null) currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            // Return
            return Task.FromResult(currentPrincipal);
        }

        public override async Task LoginAsync(ClaimsPrincipal principal)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(principal);

            #endregion

            // LoginAsync
            await _httpContextAccessor.HttpContext.LoginAsync(principal.Identity as ClaimsIdentity);

            // CurrentPrincipal
            var currentPrincipal = principal;

            // Raise
            this.OnPrincipalChanged(currentPrincipal);
        }

        public override async Task LogoutAsync()
        {
            // LoginAsync
            await _httpContextAccessor.HttpContext.LogoutAsync();

            // CurrentPrincipal
            var currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            // Raise
            this.OnPrincipalChanged(currentPrincipal);
        }
    }
}
