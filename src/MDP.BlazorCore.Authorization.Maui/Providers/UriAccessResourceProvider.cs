using MDP.AspNetCore.Authorization;
using System;

namespace MDP.BlazorCore.Authorization.Maui
{
    public class UriAccessResourceProvider : IAccessResourceProvider
    {
        // Methods
        public AccessResource Create(object resource = null)
        {
            // Uri
            var uri = resource as Uri;
            if (uri == null) return null;

            // AccessResource
            var accessResource = new AccessResource($"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}");
            if (accessResource == null) return null;

            // Return
            return accessResource;
        }
    }
}
