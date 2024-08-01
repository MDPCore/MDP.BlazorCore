using MDP.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;

namespace MDP.BlazorCore.Authorization.Maui
{
    public class InteropAccessResourceProvider : IAccessResourceProvider
    {
        // Methods
        public AccessResource Create(object resource = null)
        {
            // InteropRequest
            var interopRequest = resource as InteropRequest;
            if (interopRequest == null) return null;

            // Uri
            var uri = interopRequest.ControllerUri;
            if (uri == null) return null;

            // AccessResource
            var accessResource = new AccessResource($"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}");
            if (accessResource == null) return null;

            // Return
            return accessResource;
        }
    }
}
