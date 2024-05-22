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

            // InteropHandlerTypeList
            var interopHandlerTypeList = MDP.Reflection.Type.FindAllApplicationType();
            if (interopHandlerTypeList == null) throw new InvalidOperationException($"{nameof(interopHandlerTypeList)}=null");

            // InteropHandlerTypeList.Filter
            interopHandlerTypeList = interopHandlerTypeList.AsParallel().Where(interopHandlerType =>
            {
                // Require
                if (interopHandlerType.IsClass == false) return false;
                if (interopHandlerType.IsPublic == false) return false;
                if (interopHandlerType.IsAbstract == true) return false;
                if (interopHandlerType.IsGenericType == true) return false;
                if (interopHandlerType.IsAssignableTo(typeof(InteropHandler)) == false) return false;

                // Return
                return true;
            }).ToList();

            // InteropHandlerTypeList.Register
            foreach (var interopHandlerType in interopHandlerTypeList)
            {
                // Register
                serviceCollection.AddTransient(interopHandlerType);
            }

            // InteropMethodDictionary
            var interopMethodDictionary = new Dictionary<string, InteropMethod>(StringComparer.OrdinalIgnoreCase);
            foreach (var interopHandlerType in interopHandlerTypeList)
            {
                // MethodList
                var methodList = interopHandlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
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
