using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropManager
    {
        // Fields
        private readonly Dictionary<string, InteropResource> _interopResourceDictionary = null;

        private readonly InteropProvider _interopProvider = null;

        private readonly IAuthorizationService _authorizationService = null;

        private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider = null;

        private AuthorizationPolicy _authorizationPolicy = null;


        // Constructors
        public InteropManager(IList<InteropResource> interopResourceList, InteropProvider interopProvider, IAuthorizationService authorizationService, IAuthorizationPolicyProvider authorizationPolicyProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopResourceList);
            ArgumentNullException.ThrowIfNull(interopProvider);
            ArgumentNullException.ThrowIfNull(authorizationService);
            ArgumentNullException.ThrowIfNull(authorizationPolicyProvider);

            #endregion

            // InteropResourceDictionary
            _interopResourceDictionary = interopResourceList.ToDictionary
            (
                interopResource => interopResource.RouteTemplate,
                interopResource => interopResource,
                StringComparer.OrdinalIgnoreCase
            );

            // InteropProvider
            _interopProvider = interopProvider;

            // AuthorizationPolicyProvider
            _authorizationPolicyProvider = authorizationPolicyProvider;
        }


        // Methods
        public async Task<InteropResponse> InvokeAsync(ClaimsPrincipal principal, InteropRequest interopRequest)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(principal);
            ArgumentNullException.ThrowIfNull(interopRequest);

            #endregion

            // InteropResource
            var interopResource = this.FindInteropResource(interopRequest);
            if (interopResource == null) throw new InvalidOperationException($"{nameof(interopResource)}=null");

            // Authorization
            if (interopResource.IsAuthorizationRequired == true)
            {
                // AuthorizationPolicy
                var authorizationPolicy = await this.CreateAuthorizationPolicyAsync();
                if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");

                // AuthorizationResult
                var authorizationResult = await _authorizationService.AuthorizeAsync(principal, interopRequest, authorizationPolicy);
                if (authorizationResult.Succeeded == false) throw new UnauthorizedAccessException($"Authorization failed for resource '{interopRequest.RoutePath}'");
            }

            // InvokeAsync
            return await _interopProvider.InvokeAsync(principal, interopRequest, interopResource);
        }

        private InteropResource FindInteropResource(InteropRequest interopRequest)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopRequest);

            #endregion

            // Result
            InteropResource interopResource = null;

            // FindRoute
            if (_interopResourceDictionary.TryGetValue(interopRequest.RoutePath, out interopResource) == true) return interopResource;

            // MatchRoute
            interopResource = _interopResourceDictionary.Values.FirstOrDefault(o => o.MatchRoute(interopRequest) == true);
            if (interopResource != null) return interopResource;

            // Return
            return null;
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
