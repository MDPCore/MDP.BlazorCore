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
    public class OAuthSSOAuthenticationProviderBuilder : IAuthenticationProviderBuilder
    {
        // Fields
        private readonly IServiceProvider _serviceProvider = null;


        // Constructors
        public OAuthSSOAuthenticationProviderBuilder(IServiceProvider serviceProvider)
        {
            #region Contracts

            if (serviceProvider == null) throw new ArgumentException($"{nameof(serviceProvider)}=null");

            #endregion

            // Default
            _serviceProvider = serviceProvider;
        }


        // Properties
        public string Name { get; private set; } = "Web";


        // Methods
        public IAuthenticationProvider BuildProvider()
        {
            // Create
            return new OAuthSSOAuthenticationProvider
            (
                _serviceProvider.GetService<OAuthSSOHandler>(),
                _serviceProvider.GetService<UserManager>(),
                _serviceProvider.GetService<NavigationManager>()
            );
        }
    }
}
