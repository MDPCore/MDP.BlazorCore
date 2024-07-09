using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            IList<Type> interopServiceTypeList = null;
            {
                // FindAll
                interopServiceTypeList = MDP.Reflection.Type.FindAllApplicationType();
                if (interopServiceTypeList == null) throw new InvalidOperationException($"{nameof(interopServiceTypeList)}=null");

                // Filter
                interopServiceTypeList = interopServiceTypeList.AsParallel().Where(interopServiceType =>
                {
                    // Require
                    if (interopServiceType.IsNested == false) return false;
                    if (interopServiceType.DeclaringType == null) return false;
                    if (interopServiceType.IsAssignableTo(typeof(InteropService)) == false) return false;

                    // Return
                    return true;
                }).ToList();

                // Register
                foreach (var interopServiceType in interopServiceTypeList)
                {
                    // Add
                    serviceCollection.AddTransient(interopServiceType);
                }
            }

            // InteropResourceList
            var interopResourceList = new List<InteropResource>();
            {
                // FindAll
                foreach (var interopServiceType in interopServiceTypeList)
                {
                    // RouteAttributeList
                    var routeAttributeList = interopServiceType.DeclaringType.GetCustomAttributes<RouteAttribute>();
                    if (routeAttributeList == null) throw new InvalidOperationException($"{nameof(routeAttributeList)}=null");

                    // Add
                    interopResourceList.AddRange(routeAttributeList.Select(routeAttribute => new InteropResource(interopServiceType, routeAttribute.Template)));
                }

                // Register
                foreach (var interopResource in interopResourceList)
                {
                    // Add
                    serviceCollection.AddSingleton(interopResource);
                }
            }

            // InteropManager
            serviceCollection.AddSingleton<InteropManager>();
        }
    }
}
