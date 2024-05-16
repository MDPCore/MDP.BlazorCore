using MDP.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDP.BlazorCore.Authorization.Maui
{
    public class AuthorizationFactory : ServiceFactory<IServiceCollection, AuthorizationFactory.Setting>
    {
        // Constructors
        public AuthorizationFactory() : base("Authorization.Blazor", null, true) { }


        // Methods
        public override void ConfigureService(IServiceCollection serviceCollection, AuthorizationFactory.Setting setting)
        {
            #region Contracts

            if (serviceCollection == null) throw new ArgumentException($"{nameof(serviceCollection)}=null");
            if (setting == null) throw new ArgumentException($"{nameof(setting)}=null");

            #endregion

            // Authorization
            serviceCollection.AddAuthorizationCore(options =>
            {
                // RequirementList
                var requirementList = options.DefaultPolicy?.Requirements.ToList();
                if (requirementList == null) requirementList = new List<IAuthorizationRequirement>();

                // PolicyBuilder
                var policyBuilder = new AuthorizationPolicyBuilder();
                foreach (var requirement in requirementList)
                {
                    policyBuilder.AddRequirements(requirement);
                }
                policyBuilder.AddRequirements(new RoleAuthorizationRequirement());

                // DefaultPolicy
                options.DefaultPolicy = policyBuilder.Build();
            });

            // RoleAuthorizationHandler
            serviceCollection.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();

            // RoleAssignmentProvider

            // AccessPermissionProvider
            serviceCollection.AddSingleton<IAccessPermissionProvider>(serviceProvider =>
            {
                // AccessPermissionList
                var accessPermissionList = setting.Permissions?.Select(o => o.ToPermission()).ToList();
                if (accessPermissionList == null) accessPermissionList = new List<AccessPermission>();

                // Return
                return new DefaultAccessPermissionProvider(accessPermissionList);
            });

            // AccessResourceProvider
            serviceCollection.AddTransient<IAccessResourceProvider, BlazorAccessResourceProvider>();
        }


        // Class
        public class Setting
        {
            // Properties
            public List<PermissionSetting> Permissions { get; set; } = null;
        }

        public class PermissionSetting
        {
            // Properties
            public string RoleId { get; set; }

            public string AccessUri { get; set; }


            // Methods
            public AccessPermission ToPermission()
            {
                return new AccessPermission(this.RoleId, this.AccessUri);
            }
        }
    }
}