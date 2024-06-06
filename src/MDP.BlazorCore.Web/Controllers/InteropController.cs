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

        private readonly IServiceProvider _serviceProvider = null;


        // Constructors
        public InteropController(InteropManager interopManager, IServiceProvider serviceProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopManager);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            #endregion

            // Default
            _interopManager = interopManager;
            _serviceProvider = serviceProvider;
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
                // NavigationUri
                var navigationUri = new Uri(actionModel.ServiceUri);
                if (navigationUri == null) throw new InvalidOperationException($"{nameof(navigationUri)}=null");

                // ServiceUri
                var serviceUri = $"{navigationUri.Scheme}://{navigationUri.Host}{navigationUri.AbsolutePath}";
                if (serviceUri == null) throw new InvalidOperationException($"{nameof(serviceUri)}=null");

                // Principal
                var principal = this.User;
                if (principal == null) throw new InvalidOperationException($"{nameof(principal)}=null");

                // InvokeAsync
                var interopResponse = await _interopManager.InvokeAsync(new InteropRequest
                (
                    new Uri(serviceUri),
                    actionModel.MethodName,
                    actionModel.MethodParameters

                ), principal, _serviceProvider);
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
