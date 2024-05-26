using MDP.Registration;
using Microsoft.AspNetCore.Authorization;
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
                if (interopServiceType.IsClass == false) return false;
                if (interopServiceType.IsPublic == false) return false;
                if (interopServiceType.IsAbstract == true) return false;
                if (interopServiceType.IsGenericType == true) return false;
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

            // InteropMethodDictionary
            var interopMethodDictionary = new Dictionary<string, InteropMethod>(StringComparer.OrdinalIgnoreCase);
            foreach (var interopServiceType in interopServiceTypeList)
            {
                // MethodList
                var methodList = interopServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methodList)
                {
                    // InteropRoute
                    var routeAttribute = method.GetCustomAttribute<InteropRouteAttribute>();
                    if (routeAttribute == null) continue;

                    // InteropMethod
                    var interopMethod = new InteropMethod(routeAttribute.Template, method);
                    if (interopMethodDictionary.ContainsKey(interopMethod.Template) == true) throw new InvalidOperationException($"Duplicate route detected: {interopMethod.Template}");

                    // Add
                    interopMethodDictionary.Add(interopMethod.Template, interopMethod);
                }
            }

            // InteropManager
            serviceCollection.AddSingleton<InteropManager>(serviceProvider =>
            {
                // AuthorizationPolicyProvider
                var authorizationPolicyProvider = serviceProvider.GetService<IAuthorizationPolicyProvider>();
                if (authorizationPolicyProvider == null) throw new InvalidOperationException($"{nameof(authorizationPolicyProvider)}=null");

                // Return
                return new InteropManager(interopMethodDictionary, authorizationPolicyProvider);
            });
        }
    }
}
