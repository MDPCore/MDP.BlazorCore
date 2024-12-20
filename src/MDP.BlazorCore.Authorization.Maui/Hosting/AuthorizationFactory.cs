﻿using MDP.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using MDP.AspNetCore.Authorization;

namespace MDP.BlazorCore.Authorization.Maui
{
    public class AuthorizationFactory : ServiceFactory<IServiceCollection, AuthorizationFactory.Setting>
    {
        // Constructors
        public AuthorizationFactory() : base("Authorization", null, true) { }


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
            serviceCollection.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();

            // AccessResourceProvider
            serviceCollection.AddTransient<IAccessResourceProvider, UriAccessResourceProvider>();
            serviceCollection.AddTransient<IAccessResourceProvider, InteropAccessResourceProvider>();
            serviceCollection.AddTransient<IAccessResourceProvider, NavigationAccessResourceProvider>();

            // AccessPermissionProvider
            serviceCollection.AddSingleton<IAccessPermissionProvider>(serviceProvider =>
            {
                // AccessPermissionList
                var accessPermissionList = setting.Permissions?.Select(o => o.ToPermission()).ToList();
                if (accessPermissionList == null) accessPermissionList = new List<AccessPermission>();

                // Return
                return new DefaultAccessPermissionProvider(accessPermissionList);
            });
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
                // RoleSectionArray
                var roleSectionArray = this.RoleId.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (roleSectionArray == null) throw new InvalidOperationException($"{nameof(roleSectionArray)}=null");
                if (roleSectionArray.Length == 0) throw new InvalidOperationException($"{nameof(roleSectionArray)}=null");

                // RoleSectionArray.For
                for (int i = 0; i < roleSectionArray.Length; i++)
                {
                    // RoleSection
                    var roleSection = roleSectionArray[i];
                    if (roleSection.StartsWith("[") == true && roleSection.EndsWith("]") == true)
                    {
                        roleSectionArray[i] = roleSection.Substring(1, roleSection.Length - 2);
                    }
                }

                // RoleId
                var roleId = roleSectionArray[roleSectionArray.Length - 1];
                if (string.IsNullOrEmpty(roleId) == true) throw new InvalidOperationException($"{nameof(roleId)}=null");

                // RoleScopes
                var roleScopes = roleSectionArray.Take(roleSectionArray.Length - 1).ToList();
                if (roleScopes == null) throw new InvalidOperationException($"{nameof(roleId)}=null");

                // Return
                return new AccessPermission(roleId, roleScopes, this.AccessUri);
            }
        }
    }
}