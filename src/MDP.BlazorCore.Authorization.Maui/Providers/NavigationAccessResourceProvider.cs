using MDP.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;

namespace MDP.BlazorCore.Authorization.Maui
{
    public class NavigationAccessResourceProvider : IAccessResourceProvider
    {
        // Methods
        public AccessResource Create(object resource = null)
        {
            // NavigationManager
            var navigationManager = resource as NavigationManager;
            if (navigationManager == null) return null;

            // Uri
            var uriString = navigationManager.Uri;
            if (string.IsNullOrEmpty(uriString) == true) return null;
            var uri = new Uri(uriString);

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
