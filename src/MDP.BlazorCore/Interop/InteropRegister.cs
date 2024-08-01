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

            // InteropControllerTypeList
            IList<Type> interopControllerTypeList = null;
            {
                // FindAll
                interopControllerTypeList = MDP.Reflection.Type.FindAllApplicationType();
                if (interopControllerTypeList == null) throw new InvalidOperationException($"{nameof(interopControllerTypeList)}=null");

                // Filter
                interopControllerTypeList = interopControllerTypeList.AsParallel().Where(interopControllerType =>
                {
                    // Require
                    if (interopControllerType.IsNested == false) return false;
                    if (interopControllerType.DeclaringType == null) return false;
                    if (interopControllerType.IsAssignableTo(typeof(InteropController)) == false) return false;

                    // Return
                    return true;
                }).ToList();

                // Register
                foreach (var interopControllerType in interopControllerTypeList)
                {
                    // Add
                    serviceCollection.AddTransient(interopControllerType);
                }
            }

            // InteropResourceList
            var interopResourceList = new List<InteropResource>();
            {
                // FindAll
                foreach (var interopControllerType in interopControllerTypeList)
                {
                    // RouteAttributeList
                    var routeAttributeList = interopControllerType.DeclaringType.GetCustomAttributes<RouteAttribute>();
                    if (routeAttributeList == null) throw new InvalidOperationException($"{nameof(routeAttributeList)}=null");

                    // Add
                    interopResourceList.AddRange(routeAttributeList.Select(routeAttribute => new InteropResource(interopControllerType, routeAttribute.Template)));
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
