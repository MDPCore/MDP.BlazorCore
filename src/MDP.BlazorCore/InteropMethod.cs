using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropMethod
    {
        // Fields
        private readonly string _template = null;

        private readonly MethodInfo _method = null;

        private readonly List<string> _templateSectionList = null;


        // Constructors
        public InteropMethod(string template, MethodInfo method)
        {
            #region Contracts

            if (string.IsNullOrEmpty(template) == true) throw new ArgumentException($"{nameof(template)}=null");
            if (method == null) throw new ArgumentException($"{nameof(method)}=null");

            #endregion

            // Template
            if (template.StartsWith("/") == false) template = "/" + template;
            if (template.EndsWith("/") == true) template = template.TrimEnd('/');
            _template = template;

            // Method
            _method = method;
            
            // TemplateSectionList
            _templateSectionList = template.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (_templateSectionList.Count == 0) throw new InvalidOperationException($"{nameof(_templateSectionList)}.Count=0");
        }


        // Properties
        public string Template { get { return _template; } }


        // Methods
        public bool CanInvoke(List<string> pathSectionList)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(pathSectionList);

            #endregion

            // Require
            if (pathSectionList.Count == 0) return false;
            if (pathSectionList.Count != _templateSectionList.Count) return false;

            // Check
            for (int i = 0; i < pathSectionList.Count; i++)
            {
                // Variables
                var pathSection = pathSectionList.ElementAtOrDefault(i);
                var templateSection = _templateSectionList.ElementAtOrDefault(i);

                // Null
                if (string.IsNullOrEmpty(pathSection) == true) return false;
                if (string.IsNullOrEmpty(templateSection) == true) return false;

                // [Parameter]
                if (templateSection.StartsWith("[") == true && templateSection.EndsWith("]") == true)
                {
                    continue;
                }

                // String
                if (pathSection.Equals(templateSection, StringComparison.OrdinalIgnoreCase) == false) return false;
            }

            // Return
            return true;
        }

        public Task<object> InvokeAsync(List<string> pathSectionList, JsonDocument payload, IServiceProvider serviceProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(pathSectionList);
            ArgumentNullException.ThrowIfNull(payload);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            #endregion

            // ParameterProvider
            var parameterProvider = new InteropParameterProvider(this.CreateParameterDictionary(pathSectionList), payload);

            // Instance
            var instance = serviceProvider.GetService(_method.DeclaringType);
            if (instance == null) throw new InvalidOperationException($"{nameof(instance)}=null");

            // Invoke
            var result = MDP.Reflection.Activator.InvokeMethodAsync(instance, _method.Name, parameterProvider);
            if (result == null) throw new InvalidOperationException($"{nameof(instance)}=null");

            // Return
            return result;
        }

        private Dictionary<string, string> CreateParameterDictionary(List<string> pathSectionList)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(pathSectionList);

            #endregion

            // ParameterDictionary
            var parameterDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < pathSectionList.Count; i++)
            {
                // Variables
                var pathSection = pathSectionList.ElementAtOrDefault(i);
                var templateSection = _templateSectionList.ElementAtOrDefault(i);

                // Null
                if (string.IsNullOrEmpty(pathSection) == true) continue;
                if (string.IsNullOrEmpty(templateSection) == true) continue;

                // [Parameter]
                if (templateSection.StartsWith("[") == true && templateSection.EndsWith("]") == true)
                {
                    // Add
                    parameterDictionary.Add(templateSection.Substring(1, templateSection.Length - 2), pathSection);
                }
            }

            // Return
            return parameterDictionary;
        }
    }
}
