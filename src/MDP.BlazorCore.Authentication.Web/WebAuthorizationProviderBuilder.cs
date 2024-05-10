using MDP.Registration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace MDP.BlazorCore.Authentication.Web
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
                serviceProvider.GetService<IOptionsMonitor<AuthenticationOptions>>(),
                serviceProvider.GetService<IOptionsMonitor<CookieAuthenticationOptions>>(),
                serviceProvider.GetService<NavigationManager>()
            );
        }
    }
}
