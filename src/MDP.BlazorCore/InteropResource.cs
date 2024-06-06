using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropResource
    {
        // Fields
        private readonly Type _serviceType = null;

        private readonly string _routeTemplate = null;

        private readonly List<string> _routeTemplateSectionList = null;

        private readonly bool _isAuthorizationRequired = false;


        // Constructors
        public InteropResource(Type serviceType, RouteAttribute routeAttribute)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(serviceType);
            ArgumentNullException.ThrowIfNull(routeAttribute);

            #endregion

            // Require
            if (serviceType.IsNested == false) throw new InvalidOperationException($"{nameof(serviceType)}.IsNested=false");
            if (serviceType.DeclaringType == null) throw new InvalidOperationException($"{nameof(serviceType)}.DeclaringType=false");

            // ServiceType
            _serviceType = serviceType;

            // RouteTemplate
            _routeTemplate = routeAttribute.Template;
            if (_routeTemplate.StartsWith("/") == false) _routeTemplate = "/" + _routeTemplate;
            if (_routeTemplate.EndsWith("/") == true) _routeTemplate = _routeTemplate.TrimEnd('/');

            // RouteTemplateSectionList
            _routeTemplateSectionList = _routeTemplate.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (_routeTemplateSectionList.Count == 0) throw new InvalidOperationException($"{nameof(_routeTemplateSectionList)}.Count=0");

            // IsAuthorizationRequired
            _isAuthorizationRequired = false;
            if (serviceType.DeclaringType.GetCustomAttribute<AllowAnonymousAttribute>() != null) _isAuthorizationRequired = false;
            if (serviceType.DeclaringType.GetCustomAttribute<AuthorizeAttribute>() != null) _isAuthorizationRequired = true;
        }


        // Properties
        public string RouteTemplate { get { return _routeTemplate; } }

        public bool IsAuthorizationRequired { get { return _isAuthorizationRequired; } }


        // Methods
        public async Task<InteropResponse> InvokeAsync(InteropRequest interopRequest, ClaimsPrincipal principal, IServiceProvider serviceProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopRequest);
            ArgumentNullException.ThrowIfNull(principal);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            #endregion

            // Instance
            var instance = serviceProvider.GetService(_serviceType);
            if (instance == null) throw new InvalidOperationException($"{nameof(instance)}=null");

            // InteropService
            var interopService = instance as InteropService;
            if (interopService == null) throw new InvalidOperationException($"The instance of type {instance.GetType().FullName} is not assignable to {typeof(InteropService).FullName}.");

            // InteropService.Setup
            {
                // Properties
                interopService.User = principal;
            }

            // ParameterProvider
            var parameterProvider = this.CreateParameterProvider(interopRequest);
            if (parameterProvider == null) throw new InvalidOperationException($"{nameof(parameterProvider)}=null");

            // InvokeMethod
            var result = await MDP.Reflection.Activator.InvokeMethodAsync(interopService, interopRequest.MethodName, parameterProvider);

            // Return
            return new InteropResponse()
            {
                Succeeded = true,
                Result = result
            };
        }

        public bool MatchRoute(InteropRequest interopRequest)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopRequest);

            #endregion

            // Require
            if (interopRequest.RoutePathSectionList.Count == 0) return false;
            if (interopRequest.RoutePathSectionList.Count != _routeTemplateSectionList.Count) return false;

            // Check
            for (int i = 0; i < interopRequest.RoutePathSectionList.Count; i++)
            {
                // Variables
                var routePathSection = interopRequest.RoutePathSectionList.ElementAtOrDefault(i);
                var routeTemplateSection = _routeTemplateSectionList.ElementAtOrDefault(i);

                // Null
                if (string.IsNullOrEmpty(routePathSection) == true) return false;
                if (string.IsNullOrEmpty(routeTemplateSection) == true) return false;

                // {Parameter}
                if (routeTemplateSection.StartsWith("{") == true && routeTemplateSection.EndsWith("}") == true)
                {
                    continue;
                }

                // String
                if (routePathSection.Equals(routeTemplateSection, StringComparison.OrdinalIgnoreCase) == false) return false;
            }

            // Return
            return true;
        }

        private MDP.Reflection.ParameterProvider CreateParameterProvider(InteropRequest interopRequest)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopRequest);

            #endregion

            // Require
            if (interopRequest.RoutePathSectionList.Count == 0) throw new InvalidOperationException($"{nameof(interopRequest.RoutePathSectionList)}.Count=0");
            if (interopRequest.RoutePathSectionList.Count != _routeTemplateSectionList.Count) throw new InvalidOperationException($"{nameof(interopRequest.RoutePathSectionList)}.Count!=_routeTemplateSectionList.Count");

            // RouteParameters
            var routeParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < interopRequest.RoutePathSectionList.Count; i++)
            {
                // Variables
                var routePathSection = interopRequest.RoutePathSectionList.ElementAtOrDefault(i);
                var routeTemplateSection = _routeTemplateSectionList.ElementAtOrDefault(i);

                // Null
                if (string.IsNullOrEmpty(routePathSection) == true) continue;
                if (string.IsNullOrEmpty(routeTemplateSection) == true) continue;

                // {Parameter:Type}
                if (routeTemplateSection.StartsWith("{") == true && routeTemplateSection.EndsWith("}") == true)
                {
                    // EndIndex
                    var endIndex = routeTemplateSection.IndexOf(":") - 1;
                    if (endIndex <= -1) endIndex = routeTemplateSection.Length - 2;

                    // Add
                    routeParameters.Add(routeTemplateSection.Substring(1, endIndex), routePathSection);
                }
            }

            // Return
            return new InteropParameterProvider(routeParameters, interopRequest.MethodParameters);
        }
    }
}
