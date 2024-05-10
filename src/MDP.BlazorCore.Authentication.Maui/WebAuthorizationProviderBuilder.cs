using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDP.BlazorCore.Maui;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class WebAuthorizationProviderBuilder : IAuthorizationProviderBuilder
    {
        // Properties
        public string Name { get; private set; } = "Web";


        // Methods
        public IAuthorizationProvider BuildProvider(IServiceProvider serviceProvider)
        {
            #region Contracts

            if (serviceProvider == null) throw new ArgumentException($"{nameof(serviceProvider)}=null");

            #endregion

            // Create
            return new WebAuthorizationProvider
            (
                serviceProvider.GetService<OAuthSSOHandler>(),
                serviceProvider.GetService<UserManager>(),
                serviceProvider.GetService<NavigationManager>()
            );
        }
    }
}
