using MDP.Registration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public static class InteropRegister
    {
        // Methods
        public static void AddInteropManager(this IServiceCollection serviceCollection)
        {
            #region Contracts

            if (serviceCollection == null) throw new ArgumentException($"{nameof(serviceCollection)}=null");

            #endregion

            // InteropServiceTypeList
            var interopServiceTypeList = MDP.Reflection.Type.FindAllApplicationType();
            if (interopServiceTypeList == null) throw new InvalidOperationException($"{nameof(interopServiceTypeList)}=null");

            // InteropServiceTypeList.Filter
            interopServiceTypeList = interopServiceTypeList.AsParallel().Where(interopServiceType =>
            {
                // Require
                if (interopServiceType.IsNested == false) return false;
                if (interopServiceType.DeclaringType == null) return false;
                if (interopServiceType.IsAssignableTo(typeof(InteropService)) == false) return false;

                // Return
                return true;
            }).ToList();

            // InteropServiceTypeList.Register
            foreach (var interopServiceType in interopServiceTypeList)
            {
                // Register
                serviceCollection.AddTransient(interopServiceType);
            }

            // InteropResourceList
            var interopResourceList = new List<InteropResource>();
            foreach (var interopServiceType in interopServiceTypeList)
            {
                // RouteAttributeList
                var routeAttributeList = interopServiceType.DeclaringType.GetCustomAttributes<RouteAttribute>();
                if (routeAttributeList == null) throw new InvalidOperationException($"{nameof(routeAttributeList)}=null");

                // Add
                interopResourceList.AddRange(routeAttributeList.Select(routeAttribute => new InteropResource(interopServiceType, routeAttribute)));
            }

            // InteropManager
            serviceCollection.AddSingleton<InteropManager>(serviceProvider =>
            {
                // AuthorizationPolicyProvider
                var authorizationPolicyProvider = serviceProvider.GetService<IAuthorizationPolicyProvider>();
                if (authorizationPolicyProvider == null) throw new InvalidOperationException($"{nameof(authorizationPolicyProvider)}=null");

                // Return
                return new InteropManager(interopResourceList, authorizationPolicyProvider);
            });
        }
    }
}
