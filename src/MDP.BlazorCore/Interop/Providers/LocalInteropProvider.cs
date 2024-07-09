using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class LocalInteropProvider : InteropProvider
    {
        // Fields
        private readonly IServiceProvider _serviceProvider = null;


        // Constructors
        public LocalInteropProvider(IServiceProvider serviceProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(serviceProvider);

            #endregion

            // Default
            _serviceProvider = serviceProvider;
        }


        // Methods
        public async Task<InteropResponse> InvokeAsync(ClaimsPrincipal principal, InteropRequest interopRequest, InteropResource interopResource)
        {
            #region Contracts

            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (interopRequest == null) throw new ArgumentNullException(nameof(interopRequest));
            if (interopResource == null) throw new ArgumentNullException(nameof(interopResource));

            #endregion

            // InvokeAsync
            object invokeResult = null;
            {
                // InteropService
                var interopService = _serviceProvider.GetService(interopResource.ServiceType) as InteropService;
                if (interopService == null) throw new InvalidOperationException($"{nameof(interopService)}=null");
                if (interopService != null)
                {
                    // Properties
                    interopService.User = principal;
                }

                // InteropParameters
                var interopParameters = interopResource.CreateParameters(interopRequest);
                if (interopParameters == null) throw new InvalidOperationException($"{nameof(interopParameters)}=null");

                // InvokeMethod
                invokeResult = await MDP.Reflection.Activator.InvokeMethodAsync(interopService, interopRequest.MethodName, interopParameters);
            }

            // Return
            return new InteropResponse()
            {
                Succeeded = true,
                Result = invokeResult
            };
        }
    }
}
