using MDP.Registration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSOAuthenticationFactory : ServiceFactory<IServiceCollection, OAuthSSOAuthenticationFactory.Setting>
    {
        // Constructors
        public OAuthSSOAuthenticationFactory() : base("Authentication.Blazor", "OAuthSSO", false) { }


        // Methods
        public override void ConfigureService(IServiceCollection serviceCollection, OAuthSSOAuthenticationFactory.Setting setting)
        {
            #region Contracts

            if (serviceCollection == null) throw new ArgumentException($"{nameof(serviceCollection)}=null");
            if (setting == null) throw new ArgumentException($"{nameof(setting)}=null");

            #endregion

            // Require
            if (string.IsNullOrEmpty(setting.ClientId) == true) throw new InvalidOperationException($"{nameof(setting.ClientId)}=null");
            if (string.IsNullOrEmpty(setting.ClientUrl) == true) throw new InvalidOperationException($"{nameof(setting.ClientUrl)}=null");
            if (string.IsNullOrEmpty(setting.ServerUrl) == true) throw new InvalidOperationException($"{nameof(setting.ServerUrl)}=null");

            // OAuthSSOAuthenticationProviderBuilder
            serviceCollection.AddTransient<IAuthenticationProviderBuilder, OAuthSSOAuthenticationProviderBuilder>();

            // OAuthSSOAuthenticationProvider

            // OAuthSSOHandler
            serviceCollection.AddTransient<OAuthSSOHandler>();

            // OAuthSSOOptions
            serviceCollection.AddSingleton<OAuthSSOOptions>(serviceProvider =>
            {
                // AuthOptions
                var authOptions = new OAuthSSOOptions();
                authOptions.ClientId = setting.ClientId;
                authOptions.ClientUrl = setting.ClientUrl;
                authOptions.ServerUrl = setting.ServerUrl;

                // Return
                return authOptions;
            });
        }


        // Class
        public class Setting
        {
            // Properties
            public string ClientId { get; set; } = string.Empty;

            public string ClientUrl { get; set; } = string.Empty;

            public string ServerUrl { get; set; } = string.Empty;
        }
    }
}