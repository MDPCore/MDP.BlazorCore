using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace MDP.BlazorCore
{
    internal class InteropParameters : MDP.Reflection.ParameterProvider
    {
        // Fields
        private readonly Dictionary<string, string> _routeParameters = null;

        private readonly JsonDocument _methodParameters = null;


        // Constructors
        public InteropParameters(Dictionary<string, string> routeParameters, JsonDocument methodParameters)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(routeParameters);
            ArgumentNullException.ThrowIfNull(methodParameters);

            #endregion

            // Default
            _routeParameters = routeParameters;
            _methodParameters = methodParameters;
        }


        // Methods
        public override object GetValue(System.Type parameterType, string parameterName, bool hasDefaultValue = false, object defaultValue = null)
        {
            #region Contracts

            if (parameterType == null) throw new ArgumentException($"{nameof(parameterType)}=null");
            if (string.IsNullOrEmpty(parameterName) == true) throw new ArgumentException($"{nameof(parameterName)}=null");

            #endregion

            // ReferenceType
            if (parameterType.IsClass == true && parameterType.IsAbstract == false)
            {
                if (parameterType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) != null)
                {
                    // MethodParameters
                    if (_methodParameters.RootElement.TryGetProperty(parameterName, out JsonElement parameterElement) == true)
                    {
                        var parameter = JsonSerializer.Deserialize(parameterElement.GetRawText(), parameterType, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (parameter != null) return parameter;
                    }
                }
            }

            // ValueType
            if (parameterType.IsValueType == true || parameterType == typeof(string))
            {
                // RouteParameters
                if (_routeParameters.ContainsKey(parameterName) == true)
                {
                    if (parameterType == typeof(string))
                    {
                        var parameter = _routeParameters[parameterName];
                        if (string.IsNullOrEmpty(parameter) == false) return parameter;
                    }
                    else
                    {
                        var parameter = TypeDescriptor.GetConverter(parameterType).ConvertFromString(_routeParameters[parameterName]);
                        if (parameter != null) return parameter;
                    }
                }

                // MethodParameters
                if (_methodParameters.RootElement.TryGetProperty(parameterName, out JsonElement parameterElement) == true)
                {
                    if (parameterType == typeof(string))
                    {
                        var parameter = parameterElement.GetString();
                        if (string.IsNullOrEmpty(parameter) == false) return parameter;
                    }
                    else
                    {
                        var parameter = TypeDescriptor.GetConverter(parameterType).ConvertFromString(parameterElement.GetString());
                        if (parameter != null) return parameter;
                    }
                }
            }

            // Return
            return base.GetValue(parameterType, parameterName, hasDefaultValue, defaultValue);
        }
    }
}
