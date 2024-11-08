using MDP.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class MauiConfigurationProvider : ConfigurationProvider
    {
        // Fields
        private readonly string _environmentName = null;


        // Constructors
        public MauiConfigurationProvider(string environmentName)
        {
            #region Contracts

            if (string.IsNullOrEmpty(environmentName) == true) throw new ArgumentException($"{nameof(environmentName)}=null");

            #endregion

            // Default
            _environmentName = environmentName.ToLowerInvariant();
        }


        // Methods
        public IEnumerable<Stream> GetAllJsonStream()
        {
            // Variables
            var jsonStreamList = new List<Stream>();

            // appsettings.json
            {
                var stream = this.CreateFileStream($"appsettings.json");
                if(stream!=null) jsonStreamList.Add(stream);
            }

            // appsettings.{environmentName}.json
            {
                var stream = this.CreateFileStream($"appsettings.{_environmentName}.json");
                if (stream != null) jsonStreamList.Add(stream);
            }
            
            // Return
            return jsonStreamList;
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
