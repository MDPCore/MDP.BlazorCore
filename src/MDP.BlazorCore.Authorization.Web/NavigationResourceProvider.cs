using MDP.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authorization.Web
{
    public class NavigationResourceProvider : IResourceProvider
    {
        // Fields
        private readonly NavigationManager _navigationManager = null;


        // Constructors
        public NavigationResourceProvider(NavigationManager navigationManager)
        {
            #region Contracts

            if (navigationManager == null) throw new ArgumentException($"{nameof(navigationManager)}=null");

            #endregion

            // Default
            _navigationManager = navigationManager;
        }


        // Methods
        public Resource Create()
        {
            // Uri
            var uriString = _navigationManager.Uri;
            if (string.IsNullOrEmpty(uriString) == true) return null;
            var uri = new Uri(uriString);

            // Resource
            var resource = new Resource($"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}");
            if (resource == null) return null;

            // Return
            return resource;
        }
    }
}
