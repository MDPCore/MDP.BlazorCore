using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MDP.BlazorCore
{
    public class RoutesOptions
    {
        // Properties
        public Assembly AppAssembly { get; set; }

        public Type DefaultLayout { get; set; }

        public string LoginPath { get; set; } = "/Account/Login";

        public string LogoutPath { get; set; } = "/Account/Logout";

        public string AccessDeniedPath { get; set; } = "/Account/AccessDenied";


        // Methods
        public Assembly CreateAppAssembly()
        {
            // AppAssembly
            if (this.AppAssembly != null) return this.AppAssembly;

            // EntryAssembly
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null) return entryAssembly;

            // Return
            return null;
        }

        public Assembly[] CreateAdditionalAssemblies()
        {
            // AssemblyList
            var assemblyList = this.FindAllApplicationAssembly();
            if (assemblyList == null) return Array.Empty<Assembly>();

            // AppAssembly
            var appAssembly = this.CreateAppAssembly();
            if (appAssembly != null) assemblyList.Remove(appAssembly);

            // Return
            return assemblyList.ToArray();
        }

        private List<Assembly> FindAllApplicationAssembly()
        {
            // AssemblyDictionary
            var assemblyDictionary = new Dictionary<string, System.Reflection.Assembly>(StringComparer.OrdinalIgnoreCase);

            //  LoadedAssemblyList
            var loadedAssemblyList = AppDomain.CurrentDomain.GetAssemblies();
            if (loadedAssemblyList == null) throw new InvalidOperationException($"{nameof(loadedAssemblyList)}=null");
            foreach (var loadedAssembly in loadedAssemblyList)
            {
                if (assemblyDictionary.ContainsKey(loadedAssembly.Location) == false)
                {
                    assemblyDictionary[loadedAssembly.Location] = loadedAssembly;
                }
            }

            // FileAssemblyPathList
            var fileAssemblyPathList = MDP.IO.File.GetAllFilePath("*.dll");
            if (fileAssemblyPathList == null) throw new InvalidOperationException($"{nameof(fileAssemblyPathList)}=null");
            foreach (var fileAssemblyPath in fileAssemblyPathList)
            {
                if (assemblyDictionary.ContainsKey(fileAssemblyPath) == false)
                {
                    // FileAssembly
                    var fileAssembly = System.Reflection.Assembly.LoadFrom(fileAssemblyPath);
                    if (fileAssembly == null) throw new InvalidOperationException($"{nameof(fileAssembly)}=null");

                    // Add
                    assemblyDictionary[fileAssembly.Location] = fileAssembly;
                }
            }

            // EntryAssembly
            var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                if (assemblyDictionary.ContainsKey(entryAssembly.Location) == false)
                {
                    assemblyDictionary[entryAssembly.Location] = entryAssembly;
                }
            }

            // AssemblyList 
            var assemblyList = assemblyDictionary.Values.Where(assembly =>
            {
                // AssemblyName
                var assemblyName = System.IO.Path.GetFileName(assembly.Location);
                if (string.IsNullOrEmpty(assemblyName) == true) return false;

                // Filter
                if (assemblyName.StartsWith("System") == true) return false;
                if (assemblyName.StartsWith("Microsoft") == true) return false;
                if (assemblyName.StartsWith("_Microsoft") == true) return false;

                // Filter(MAUI)
                if (assemblyName.StartsWith("Xamarin") == true) return false;
                if (assemblyName.StartsWith("Mono") == true) return false;

                // Filter(MAUI.WinRT)
                if (assemblyName.StartsWith("WinRT") == true) return false;

                // Filter(MAUI.Android)
                if (assemblyName.StartsWith("Java") == true) return false;
                if (assemblyName.StartsWith("Google") == true) return false;
                if (assemblyName.StartsWith("Windows") == true) return false;
                if (assemblyName.StartsWith("mscorlib.dll") == true) return false;
                if (assemblyName.StartsWith("netstandard.dll") == true) return false;
                if (assemblyName.StartsWith("Jsr305Binding.dll") == true) return false;

                // Return
                return true;
            }).ToList();

            // Return
            return assemblyList;
        }
    }
}
