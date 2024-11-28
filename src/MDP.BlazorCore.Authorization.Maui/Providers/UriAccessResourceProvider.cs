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

            // Scheme
            var scheme = "app";

            // Host
            var host = "localhost";

            // AbsolutePath
            var absolutePath = uri.AbsolutePath;
            if (string.IsNullOrEmpty(absolutePath) == true) absolutePath = "/";

            // AccessResource
            var accessResource = new AccessResource($"{scheme}://{host}{absolutePath}");
            if (accessResource == null) return null;

            // Return
            return accessResource;
        }
    }
}
