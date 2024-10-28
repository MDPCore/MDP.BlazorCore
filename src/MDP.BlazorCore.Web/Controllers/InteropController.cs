using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Web
{
    public class InteropController : Controller
    {
        // Fields
        private readonly InteropManager _interopManager = null;


        // Constructors
        public InteropController(InteropManager interopManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopManager);

            #endregion

            // Default
            _interopManager = interopManager;
        }


        // Methods
        [AllowAnonymous]
        [Route("/.blazor/interop/invokeAsync")]
        public async Task<IActionResult> InvokeAsync([FromBody] InvokeActionModel actionModel)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(actionModel);

            #endregion
                        
            // Execute
            try
            {
                // Require
                if (string.IsNullOrEmpty(actionModel.ControllerUri) == true) throw new InvalidOperationException($"{nameof(actionModel.ControllerUri)}=null");
                if (string.IsNullOrEmpty(actionModel.ActionName) == true) throw new InvalidOperationException($"{nameof(actionModel.ActionName)}=null");
                if (actionModel.ActionParameters == null) throw new InvalidOperationException($"{nameof(actionModel.ActionParameters)}=null");

                // Principal
                var principal = this.User;
                if (principal == null) throw new InvalidOperationException($"{nameof(principal)}=null");

                // NavigationUri
                var navigationUri = new Uri(actionModel.ControllerUri);
                if (navigationUri == null) throw new InvalidOperationException($"{nameof(navigationUri)}=null");

                // ControllerUri
                var controllerUri = $"{navigationUri.Scheme}://{navigationUri.Host}{navigationUri.AbsolutePath}";
                if (controllerUri == null) throw new InvalidOperationException($"{nameof(controllerUri)}=null");

                // InvokeAsync
                var interopResponse = await _interopManager.InvokeAsync(principal, new InteropRequest
                (
                    new Uri(controllerUri),
                    actionModel.ActionName,
                    actionModel.ActionParameters
                ));
                if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");

                // StatusCode
                switch (interopResponse.StatusCode)
                {
                    // Return
                    case InteropStatusCode.OK: return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.OK };
                    case InteropStatusCode.BadRequest: return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.BadRequest };
                    case InteropStatusCode.Unauthorized: return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.Unauthorized };
                    case InteropStatusCode.Forbidden: return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.Forbidden };
                    case InteropStatusCode.NotFound: return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.NotFound };

                    // Default
                    default:
                        return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.InternalServerError };
                }
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
                return new ObjectResult(interopResponse) { StatusCode = (int)HttpStatusCode.InternalServerError };
            }
        }


        // Class
        public class InvokeActionModel 
        {        
            // Properties
            public string ControllerUri { get; set; } = null;

            public string ActionName { get; set; } = null;

            public JsonDocument ActionParameters { get; set; } = null;        
        }
    }
}
