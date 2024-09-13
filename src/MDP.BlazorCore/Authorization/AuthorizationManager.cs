using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class AuthorizationManager
    {
        // Fields
        private readonly IAuthorizationService _authorizationService = null;

        private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider = null;

        private AuthorizationPolicy _authorizationPolicy = null;


        // Constructors
        public AuthorizationManager(IAuthorizationService authorizationService, IAuthorizationPolicyProvider authorizationPolicyProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authorizationService);
            ArgumentNullException.ThrowIfNull(authorizationPolicyProvider);

            #endregion

            // AuthorizationService
            _authorizationService = authorizationService;

            // AuthorizationPolicyProvider
            _authorizationPolicyProvider = authorizationPolicyProvider;
        }


        // Methods
        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal principal, object resource = null)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(principal);

            #endregion

            // AuthorizationPolicy
            var authorizationPolicy = await this.CreateAuthorizationPolicyAsync();
            if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");

            // AuthorizationResult
            var authorizationResult = await _authorizationService.AuthorizeAsync(principal, resource, authorizationPolicy);
            if (authorizationResult == null) throw new InvalidOperationException($"{nameof(authorizationResult)}=null");

            // Return
            return authorizationResult;
        }

        private async Task<AuthorizationPolicy> CreateAuthorizationPolicyAsync()
        {
            // Require
            if (_authorizationPolicy != null) return _authorizationPolicy;

            // AuthorizationPolicy
            var authorizationPolicy = await _authorizationPolicyProvider.GetDefaultPolicyAsync();
            if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");
            _authorizationPolicy = authorizationPolicy;

            // Return
            return _authorizationPolicy;
        }
    }
}
