using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    internal static class MauiReferenceAssemblies
    {
        // Methods
        public static void Initialize(string fileName = "References.txt")
        {
            // Stream
            var stream = CreateFileStream(fileName);
            if (stream == null) return;

            // StreamReader
            using (stream)
            using (var streamReader = new StreamReader(stream))
            {
                // AssemblyString
                foreach (var assemblyString in streamReader.ReadToEnd().Split('\n'))
                {
                    // Require
                    if (string.IsNullOrEmpty(assemblyString) == true) continue;
                    if (MDP.Reflection.Assembly.IsApplicationAssembly(assemblyString) == false) continue;

                    // Assembly
                    try
                    {
                        var assembly = System.Reflection.Assembly.Load(assemblyString);
                        Console.WriteLine($"Load assembly succeed: assembly={assemblyString}");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"Load assembly failed: assembly={assemblyString}, error={exception.Message}");
                    }
                }
            }
        }

        private static Stream CreateFileStream(string fileName)
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