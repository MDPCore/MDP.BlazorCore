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
            // InteropResource
            var interopResource = resource as InteropResource;
            if (interopResource == null) return null;

            // Uri
            var uri = interopResource.Uri;
            if (uri == null) return null;

            // AccessResource
            var accessResource = new AccessResource($"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}");
            if (accessResource == null) return null;

            // Return
            return accessResource;
        }
    }
}
