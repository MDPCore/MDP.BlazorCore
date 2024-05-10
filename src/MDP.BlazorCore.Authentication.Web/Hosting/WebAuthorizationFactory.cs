using MDP.Registration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MDP.BlazorCore.Authentication.Web
{
    public class OAuthSSOAuthenticationFactory : ServiceFactory<IServiceCollection, OAuthSSOAuthenticationFactory.Setting>
    {
        // Constructors
        public OAuthSSOAuthenticationFactory() : base("Authentication.Blazor", null, true) { }


        // Methods
        public override void ConfigureService(IServiceCollection serviceCollection, OAuthSSOAuthenticationFactory.Setting setting)
        {
            #region Contracts

            if (serviceCollection == null) throw new ArgumentException($"{nameof(serviceCollection)}=null");
            if (setting == null) throw new ArgumentException($"{nameof(setting)}=null");

            #endregion

            // AuthorizationProviderBuilder
            serviceCollection.AddTransient<IAuthorizationProviderBuilder, WebAuthorizationProviderBuilder>();
        }


        // Class
        public class Setting
        {
            // Properties

        }
    }
}