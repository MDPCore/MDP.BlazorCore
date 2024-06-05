using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropManager
    {
        // Fields
        private readonly Dictionary<string, InteropResource> _interopResourceDictionary = null;

        private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider = null;

        private AuthorizationPolicy _authorizationPolicy = null;


        // Constructors
        public InteropManager(List<InteropResource> interopResourceList, IAuthorizationPolicyProvider authorizationPolicyProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopResourceList);
            ArgumentNullException.ThrowIfNull(authorizationPolicyProvider);

            #endregion

            // InteropResourceDictionary
            _interopResourceDictionary = interopResourceList.ToDictionary
            (
                interopResource => interopResource.RouteTemplate,
                interopResource => interopResource,
                StringComparer.OrdinalIgnoreCase
            );

            // AuthorizationPolicyProvider
            _authorizationPolicyProvider = authorizationPolicyProvider;
        }


        // Methods
        public async Task<InteropResponse> InvokeAsync(InteropRequest interopRequest, IServiceProvider serviceProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopRequest);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            #endregion

            // InteropResource
            var interopResource = this.FindInteropResource(interopRequest);
            if (interopResource == null) throw new InvalidOperationException($"{nameof(interopResource)}=null");

            // AuthenticationStateProvider
            var authenticationStateProvider = serviceProvider.GetService<AuthenticationStateProvider>();
            if (authenticationStateProvider == null) throw new InvalidOperationException($"{nameof(authenticationStateProvider)}=null");

            // Principal
            var principal = (await authenticationStateProvider.GetAuthenticationStateAsync())?.User;
            if (principal == null) principal = new ClaimsPrincipal(new ClaimsIdentity());

            // IsAuthorizationRequired
            if (interopResource.IsAuthorizationRequired == true)
            {
                // AuthorizationService
                var authorizationService = serviceProvider.GetService<IAuthorizationService>();
                if (authorizationService == null) throw new InvalidOperationException($"{nameof(authorizationService)}=null");

                // AuthorizationPolicy
                var authorizationPolicy = await this.CreateAuthorizationPolicyAsync();
                if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");

                // AuthorizationResult
                var authorizationResult = await authorizationService.AuthorizeAsync(principal, interopRequest, authorizationPolicy);
                if (authorizationResult.Succeeded == false) throw new UnauthorizedAccessException($"Authorization failed for resource '{interopRequest.RoutePath}'");
            }

            // InvokeAsync
            return await interopResource.InvokeAsync(interopRequest, principal, serviceProvider);
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

            // Create
            var authorizationPolicy = await _authorizationPolicyProvider.GetDefaultPolicyAsync();
            if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");
            _authorizationPolicy = authorizationPolicy;

            // Return
            return _authorizationPolicy;
        }
    }
}
