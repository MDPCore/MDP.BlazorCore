using MDP.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;

namespace MDP.BlazorCore.Authorization.Web
{
    public class BlazorAccessResourceProvider : IAccessResourceProvider
    {
        // Fields
        private readonly NavigationManager _navigationManager = null;


        // Constructors
        public BlazorAccessResourceProvider(NavigationManager navigationManager)
        {
            #region Contracts

            if (navigationManager == null) throw new ArgumentException($"{nameof(navigationManager)}=null");

            #endregion

            // Default
            _navigationManager = navigationManager;
        }


        // Methods
        public AccessResource Create()
        {
            // Uri
            var uriString = _navigationManager.Uri;
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
