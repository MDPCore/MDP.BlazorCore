using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Web
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

            // Execute
            try
            {
                // InteropController
                var interopController = _serviceProvider.GetService(interopResource.ServiceType) as MDP.BlazorCore.InteropController;
                if (interopController == null)
                {
                    // NotFound
                    return new InteropResponse()
                    {
                        StatusCode = InteropStatusCode.NotFound,
                        Result = null,
                        ErrorMessage = $"Not found for resource: {interopRequest.RoutePath}/{interopRequest.ActionName}"
                    };
                }
                if (interopController != null) interopController.User = principal;

                // InteropParameters
                var interopParameters = interopResource.CreateParameters(interopRequest);
                if (interopParameters == null)
                {
                    // BadRequest
                    return new InteropResponse()
                    {
                        StatusCode = InteropStatusCode.BadRequest,
                        Result = null,
                        ErrorMessage = $"Bad request for resource: {interopRequest.RoutePath}/{interopRequest.ActionName}"
                    };
                }

                // InteropResponse
                var invokeResult = await MDP.Reflection.Activator.InvokeMethodAsync(interopController, interopRequest.ActionName, interopParameters);
                var interopResponse = new InteropResponse()
                {
                    StatusCode = InteropStatusCode.OK,
                    Result = invokeResult,
                    ErrorMessage = null
                };

                // Return
                return interopResponse;
            }
            catch (Exception exception)
            {
                // Require
                while (exception.InnerException != null) exception = exception.InnerException;

                // InteropResponse
                var interopResponse = new InteropResponse()
                {
                    StatusCode = InteropStatusCode.InternalServerError,
                    Result = null,
                    ErrorMessage = exception.Message
                };

                // Return
                return interopResponse;
            }
        }
    }
}
