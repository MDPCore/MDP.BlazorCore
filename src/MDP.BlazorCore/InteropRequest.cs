using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropRequest
    {
        // Constructors
        public InteropRequest(Uri uri, JsonDocument payload, ClaimsPrincipal principal, IServiceProvider serviceProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNull(payload);
            ArgumentNullException.ThrowIfNull(principal);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            #endregion

            // Default
            this.Uri = uri;
            this.Payload = payload;
            this.Principal = principal;
            this.ServiceProvider = serviceProvider;

            // Resource
            this.Resource = new InteropResource(uri);
        }


        // Properties
        public Uri Uri { get; private set; }
        
        public JsonDocument Payload { get; private set; }

        public ClaimsPrincipal Principal { get; private set; }

        public IServiceProvider ServiceProvider { get; private set; }

        public InteropResource Resource { get; private set; }
    }
}
