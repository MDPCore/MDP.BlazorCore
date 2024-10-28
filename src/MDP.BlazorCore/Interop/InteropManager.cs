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

        private readonly AuthorizationManager _authorizationManager = null;


        // Constructors
        public InteropManager(IList<InteropResource> interopResourceList, InteropProvider interopProvider, AuthorizationManager authorizationManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopResourceList);
            ArgumentNullException.ThrowIfNull(interopProvider);
            ArgumentNullException.ThrowIfNull(authorizationManager);

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

            // AuthorizationManager
            _authorizationManager = authorizationManager;
        }


        // Methods
        public async Task<InteropResponse> InvokeAsync(ClaimsPrincipal principal, InteropRequest interopRequest)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(principal);
            ArgumentNullException.ThrowIfNull(interopRequest);

            #endregion

            // Execute
            try
            {
                // InteropResource
                var interopResource = this.FindInteropResource(interopRequest);
                if (interopResource == null)
                {
                    // NotFound
                    return new InteropResponse()
                    {
                        StatusCode = InteropStatusCode.NotFound,
                        Result = null,
                        ErrorMessage = $"Not found for resource: {interopRequest.RoutePath}/{interopRequest.ActionName}"
                    };
                }

                // Authorization
                if (interopResource.IsAuthorizationRequired == true)
                {
                    // IsAuthenticated
                    if (principal.Identity?.IsAuthenticated != true)
                    {
                        // Unauthorized
                        return new InteropResponse()
                        {
                            StatusCode = InteropStatusCode.Unauthorized,
                            Result = null,
                            ErrorMessage = $"Authentication failed for resource: {interopRequest.RoutePath}/{interopRequest.ActionName}"
                        };
                    }

                    // AuthorizeAsync
                    var authorizationResult = await _authorizationManager.AuthorizeAsync(principal, interopRequest);
                    if (authorizationResult == null) throw new InvalidOperationException($"{nameof(authorizationResult)}=null");
                    if (authorizationResult.Succeeded == false)
                    {
                        // Forbidden
                        return new InteropResponse()
                        {
                            StatusCode = InteropStatusCode.Forbidden,
                            Result = null,
                            ErrorMessage = $"Authorization failed for resource: {interopRequest.RoutePath}/{interopRequest.ActionName}"
                        };
                    }
                }

                // InteropResponse
                var interopResponse = await _interopProvider.InvokeAsync(principal, interopRequest, interopResource);
                if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");

                // Return
                return interopResponse;
            }
            catch (Exception exception)
            {
                // Require
                while (exception.InnerException != null) exception = exception.InnerException;

                // InteropResponse
                return new InteropResponse()
                {
                    StatusCode = InteropStatusCode.InternalServerError,
                    Result = null,
                    ErrorMessage = exception.Message
                };
            }
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
    }
}
