using MDP.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System;

namespace MDP.BlazorCore.Authorization.Web
{
    public class BlazorAccessResourceProvider : IAccessResourceProvider
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

            // AccessResource
            var accessResource = new AccessResource($"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}");
            if (accessResource == null) return null;

            // Return
            return accessResource;
        }
    }
}
