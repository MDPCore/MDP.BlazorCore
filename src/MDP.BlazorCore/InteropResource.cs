using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropResource
    {
        // Constructors
        public InteropResource(Uri uri)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(uri);

            #endregion

            // Default
            this.Uri = uri;
        }


        // Properties
        public Uri Uri { get; private set; }
    }
}
