using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MDP.BlazorCore
{
    public class InteropParameterProvider : MDP.Reflection.ParameterProvider
    {
        // Fields
        private readonly Dictionary<string, string> _parameterDictionary = null;

        private readonly JsonDocument _parameterDocument = null;


        // Constructors
        public InteropParameterProvider(Dictionary<string, string> parameterDictionary, JsonDocument parameterDocument)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(parameterDictionary);
            ArgumentNullException.ThrowIfNull(parameterDocument);

            #endregion

            // Default
            _parameterDictionary = parameterDictionary;
            _parameterDocument = parameterDocument;
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
                    // ParameterDocument
                    if (_parameterDocument.RootElement.TryGetProperty(parameterName, out JsonElement parameterElement)==true)
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
                // ParameterDocument
                if (_parameterDocument.RootElement.TryGetProperty(parameterName, out JsonElement parameterElement) == true)
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

                // ParameterDictionary
                if (_parameterDictionary.ContainsKey(parameterName) == true)
                {
                    if (parameterType == typeof(string))
                    {
                        var parameter = _parameterDictionary[parameterName];
                        if (string.IsNullOrEmpty(parameter) == false) return parameter;
                    }
                    else
                    {
                        var parameter = TypeDescriptor.GetConverter(parameterType).ConvertFromString(_parameterDictionary[parameterName]);
                        if (parameter != null) return parameter;
                    }
                }
            }

            // Return
            return base.GetValue(parameterType, parameterName, hasDefaultValue, defaultValue);
        }
    }
}
