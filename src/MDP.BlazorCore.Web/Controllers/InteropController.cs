using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task<InteropResponse> InvokeAsync([FromBody] InvokeActionModel actionModel)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(actionModel);

            #endregion

            // Require
            if (string.IsNullOrEmpty(actionModel.ControllerUri) == true) throw new InvalidOperationException($"{nameof(actionModel.ControllerUri)}=null");
            if (string.IsNullOrEmpty(actionModel.ActionName) == true) throw new InvalidOperationException($"{nameof(actionModel.ActionName)}=null");
            if (actionModel.ActionParameters == null) throw new InvalidOperationException($"{nameof(actionModel.ActionParameters)}=null");

            // Execute
            try
            {
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

                // Return
                return interopResponse;
            }
            catch (Exception exception)
            {
                // Require
                while (exception.InnerException != null) exception = exception.InnerException;

                // Return
                return new InteropResponse()
                {
                    Succeeded = false,
                    ErrorMessage = exception.Message
                };
            }
        }

        public class InvokeActionModel 
        {        
            // Properties
            public string ControllerUri { get; set; } = null;

            public string ActionName { get; set; } = null;

            public JsonDocument ActionParameters { get; set; } = null;        
        }
    }
}
