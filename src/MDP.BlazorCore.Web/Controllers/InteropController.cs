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
        [Route("/.blazor/interop/invoke")]
        public async Task<InteropResponse> InvokeAsync([FromBody] InvokeActionModel actionModel)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(actionModel);

            #endregion

            // Require
            if (string.IsNullOrEmpty(actionModel.ServiceUri) == true) throw new InvalidOperationException($"{nameof(actionModel.ServiceUri)}=null");
            if (string.IsNullOrEmpty(actionModel.MethodName) == true) throw new InvalidOperationException($"{nameof(actionModel.MethodName)}=null");
            if (actionModel.MethodParameters == null) throw new InvalidOperationException($"{nameof(actionModel.MethodParameters)}=null");

            // Execute
            try
            {
                // Principal
                var principal = this.User;
                if (principal == null) throw new InvalidOperationException($"{nameof(principal)}=null");

                // NavigationUri
                var navigationUri = new Uri(actionModel.ServiceUri);
                if (navigationUri == null) throw new InvalidOperationException($"{nameof(navigationUri)}=null");

                // ServiceUri
                var serviceUri = $"{navigationUri.Scheme}://{navigationUri.Host}{navigationUri.AbsolutePath}";
                if (serviceUri == null) throw new InvalidOperationException($"{nameof(serviceUri)}=null");

                // InvokeAsync
                var interopResponse = await _interopManager.InvokeAsync(principal, new InteropRequest
                (
                    new Uri(serviceUri),
                    actionModel.MethodName,
                    actionModel.MethodParameters

                ));
                if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");

                // Return
                return interopResponse;
            }
            catch (Exception exception)
            {
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
            public string ServiceUri { get; set; } = null;

            public string MethodName { get; set; } = null;

            public JsonDocument MethodParameters { get; set; } = null;        
        }
    }
}
