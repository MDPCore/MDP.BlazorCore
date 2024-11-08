using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MDP.BlazorCore.Maui
{
    public class MauiEnvironmentVariables
    {
        // Fields
        private readonly Dictionary<string, string> _variableDictionary = null;


        // Constructors
        public MauiEnvironmentVariables(string fileName = "Environment.ini")
        {
            #region Contracts

            if (string.IsNullOrEmpty(fileName) == true) throw new ArgumentException($"{nameof(fileName)}=null");

            #endregion

            // Default
            _variableDictionary = this.CreateVariableDictionary(fileName);
            if (_variableDictionary == null) throw new InvalidOperationException($"{nameof(_variableDictionary)}=null");
        }


        // Methods
        public string GetVariable(string name)
        {
            #region Contracts

            if (string.IsNullOrEmpty(name) == true) throw new ArgumentException($"{nameof(name)}=null");

            #endregion

            // Require
            if (_variableDictionary.ContainsKey(name) == false) return null;

            // Return
            return _variableDictionary[name];
        }

        private Dictionary<string, string> CreateVariableDictionary(string fileName)
        {
            #region Contracts

            if (string.IsNullOrEmpty(fileName) == true) throw new ArgumentException($"{nameof(fileName)}=null");

            #endregion

            // Variables
            var variableDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Stream
            var stream = this.CreateFileStream(fileName);
            if (stream == null) return variableDictionary;

            // StreamReader
            using (stream)
            using (var streamReader = new StreamReader(stream))
            {
                // VariableString
                foreach (var variableString in streamReader.ReadToEnd().Split('\n'))
                {
                    // VariableParameter
                    var variableParameter = variableString.Split('=');
                    if (variableParameter.Length != 2) continue;

                    // VariableName
                    var variableName = variableParameter[0].Trim();
                    if (string.IsNullOrEmpty(variableName) == true) continue;

                    // VariableValue
                    var variableValue = variableParameter[1].Trim();
                    if (string.IsNullOrEmpty(variableValue) == true) continue;

                    // Add
                    variableDictionary[variableName] = variableValue;
                }
            }

            // Return
            return variableDictionary;
        }

        private Stream CreateFileStream(string fileName)
        {
            // Variables
            Stream resultStream = null;

            // LoadFile
            try
            {
                FileSystem.OpenAppPackageFileAsync(fileName).ContinueWith(task =>
                {
                    // Require
                    if (task.IsFaulted == true) return;
                    if (task.Result == null) return;

                    // FileStream
                    using (var fileStream = task.Result)
                    {
                        // ResultStream
                        resultStream = new MemoryStream();
                        fileStream.CopyTo(resultStream);
                        resultStream.Position = 0;
                    }
                }).Wait();
            }
            catch
            {
                // Nothing
                resultStream = null;
            }

            // Return
            return resultStream;
        }
    }
}
