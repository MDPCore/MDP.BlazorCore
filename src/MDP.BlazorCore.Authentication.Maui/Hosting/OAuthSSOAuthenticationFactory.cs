using MDP.BlazorCore.Maui;
using MDP.Registration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSOAuthenticationFactory : ServiceFactory<IServiceCollection, OAuthSSOAuthenticationFactory.Setting>
    {
        // Constructors
        public OAuthSSOAuthenticationFactory() : base("Authentication", "OAuthSSO", false) { }


        // Methods
        public override void ConfigureService(IServiceCollection serviceCollection, OAuthSSOAuthenticationFactory.Setting setting)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(serviceCollection);
            ArgumentNullException.ThrowIfNull(setting);

            #endregion

            // Require
            if (string.IsNullOrEmpty(setting.ClientId) == true) throw new InvalidOperationException($"{nameof(setting.ClientId)}=null");
            if (string.IsNullOrEmpty(setting.ClientScheme) == true) throw new InvalidOperationException($"{nameof(setting.ClientScheme)}=null");
            if (string.IsNullOrEmpty(setting.ServerUrl) == true) throw new InvalidOperationException($"{nameof(setting.ServerUrl)}=null");

            // OAuthSSOProvider
            serviceCollection.AddSingleton<OAuthSSOProvider>();
            serviceCollection.AddSingleton<IActivationProvider>(serviceProvider => serviceProvider.GetRequiredService<OAuthSSOProvider>());
            serviceCollection.AddSingleton<IAuthenticationProvider>(serviceProvider => serviceProvider.GetRequiredService<OAuthSSOProvider>());

            // OAuthSSOOptions
            serviceCollection.AddTransient<OAuthSSOOptions>(serviceProvider =>
            {
                // AuthOptions
                var authOptions = new OAuthSSOOptions();
                authOptions.ClientId = setting.ClientId;
                authOptions.ClientScheme = setting.ClientScheme;
                authOptions.ServerUrl = setting.ServerUrl;
                authOptions.UseCookies = setting.UseCookies;
                authOptions.IgnoreServerCertificate = setting.IgnoreServerCertificate;

                // Return
                return authOptions;
            });
        }


        // Class
        public class Setting
        {
            // Properties
            public string ClientId { get; set; } = string.Empty;

            public string ClientScheme { get; set; } = string.Empty;

            public string ServerUrl { get; set; } = string.Empty;

            public bool UseCookies { get; set; } = false;

            public bool IgnoreServerCertificate { get; set; } = false;
        }
    }
}