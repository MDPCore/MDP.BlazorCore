using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class PageComponent : PageComponent<object>
    {
        
    }

    public class PageComponent<TModel> : ComponentBase where TModel : class, new()
    {
        // Properties
        [Parameter]
        public bool Initialized { get; set; } = false;

        [Parameter]
        public TModel Model { get; set; } = null;

        [Parameter]
        public PageContext Context { get; set; } = null;

        [Parameter]
        public ClaimsPrincipal User { get; set; } = null;

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public RoutesOptions RoutesOptions { get; set; }

        [Inject]
        public InteropManager InteropManager { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }


        // Methods
        protected override async Task OnInitializedAsync()
        {
            // Variables
            Exception pageError = null;

            // Execute
            try
            {
                // Base
                await Task.Yield();
                await base.OnInitializedAsync();

                // Principal
                var principal = (await this.AuthenticationStateProvider.GetAuthenticationStateAsync())?.User;
                if (principal == null) throw new InvalidOperationException($"{nameof(principal)}=null");
                if (principal != null) this.User = principal;

                // NavigationUri
                var navigationUri = new Uri(this.NavigationManager.Uri);
                if (navigationUri == null) throw new InvalidOperationException($"{nameof(navigationUri)}=null");

                // ControllerUri
                var controllerUri = $"{navigationUri.Scheme}://{navigationUri.Host}{navigationUri.AbsolutePath}";
                if (controllerUri == null) throw new InvalidOperationException($"{nameof(controllerUri)}=null");

                // HasInitialize
                var hasInitialize = this.GetType().GetNestedTypes(BindingFlags.Public).Any(nestedType =>
                {
                    // Require
                    if (nestedType.IsSubclassOf(typeof(InteropController)) == false) return false;
                    if (nestedType.GetMethod(nameof(OnInitializedAsync)) == null) return false;

                    // Return
                    return true;
                });

                // OnInitializedAsync
                if (hasInitialize == true)
                {
                    // InvokeAsync
                    var interopResponse = await this.InteropManager.InvokeAsync(principal, new InteropRequest
                    (
                        new Uri(controllerUri),
                        nameof(OnInitializedAsync)
                    ));
                    if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");

                    // StatusCode
                    switch(interopResponse.StatusCode)
                    {
                        // OK
                        case InteropStatusCode.OK:
                            break;

                        // Unauthorized
                        case InteropStatusCode.Unauthorized:
                            this.NavigationManager.NavigateToLogin(this.RoutesOptions);
                            break;

                        // Forbidden
                        case InteropStatusCode.Forbidden: 
                            this.NavigationManager.NavigateToAccessDenied(this.RoutesOptions);
                            break;

                        // Other
                        default: throw new ApplicationException(interopResponse.ErrorMessage);
                    }

                    // PageModel
                    this.Model = interopResponse.Result as TModel;
                }
            }
            catch (Exception exception)
            {
                // Require
                while (exception.InnerException != null) exception = exception.InnerException;

                // PageError
                pageError = exception;
            }
            finally
            {
                // Initialized
                this.Initialized = true;

                // Context
                this.Context = new PageContext(this.CreatePageData(), pageError);
            }            
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // PageLoading
            if (firstRender == true)
            {
                // Invoke
                await this.JSRuntime.InvokeVoidAsync("eval", "mdp.blazorCore.pageManager.dispatchPageLoading();");

                // Return
                return;
            }            

            // PageLoaded
            if (this.Initialized == true)
            {
                // Invoke
                await this.JSRuntime.InvokeVoidAsync("eval", "mdp.blazorCore.pageManager.dispatchPageLoaded();");

                // Return
                return;
            }
        }

        private Dictionary<string, object> CreatePageData()
        {
            // Require
            if (this.Model == null) return null;

            // Create
            var pageData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var propertyInfo in this.Model.GetType().GetProperties())
            {
                if (propertyInfo.GetCustomAttribute<PageDataAttribute>() != null)
                {
                    pageData[propertyInfo.Name] = propertyInfo.GetValue(this.Model);
                }
            }

            // Return
            return pageData;
        }
    }
}
