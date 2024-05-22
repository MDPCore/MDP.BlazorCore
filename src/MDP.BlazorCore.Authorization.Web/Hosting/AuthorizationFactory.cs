using MDP.AspNetCore.Authorization;
using MDP.Registration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MDP.BlazorCore.Authorization.Web
{
    public class AuthorizationFactory : ServiceFactory<WebApplicationBuilder, AuthorizationFactory.Setting>
    {
        // Constructors
        public AuthorizationFactory() : base("Authorization", null, true) { }


        // Methods
        public override void ConfigureService(WebApplicationBuilder applicationBuilder, AuthorizationFactory.Setting setting)
        {
            #region Contracts

            if (applicationBuilder == null) throw new ArgumentException($"{nameof(applicationBuilder)}=null");
            if (setting == null) throw new ArgumentException($"{nameof(setting)}=null");

            #endregion

            // AccessResourceProvider
            applicationBuilder.Services.AddTransient<IAccessResourceProvider, BlazorAccessResourceProvider>();
            applicationBuilder.Services.AddTransient<IAccessResourceProvider, InteropAccessResourceProvider>();
        }


        // Class
        public class Setting
        {
            // Properties
            
        }
    }
}