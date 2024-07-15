using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public TModel Model { get; set; }

        [Parameter]
        public object PageData { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public InteropManager InteropManager { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }


        // Methods
        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Base
                await Task.Yield();
                await base.OnInitializedAsync();

                // Principal
                var principal = (await this.AuthenticationStateProvider.GetAuthenticationStateAsync())?.User;
                if (principal == null) throw new InvalidOperationException($"{nameof(principal)}=null");

                // NavigationUri
                var navigationUri = new Uri(this.NavigationManager.Uri);
                if (navigationUri == null) throw new InvalidOperationException($"{nameof(navigationUri)}=null");

                // ServiceUri
                var serviceUri = $"{navigationUri.Scheme}://{navigationUri.Host}{navigationUri.AbsolutePath}";
                if (serviceUri == null) throw new InvalidOperationException($"{nameof(serviceUri)}=null");

                // HasInitialize
                var hasInitialize = this.GetType().GetNestedTypes(BindingFlags.Public).Any(nestedType =>
                {
                    // Require
                    if (nestedType.IsSubclassOf(typeof(InteropService)) == false) return false;
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
                        new Uri(serviceUri),
                        nameof(OnInitializedAsync)
                    ));
                    if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");
                    if (interopResponse.Succeeded == false) throw new InvalidOperationException(interopResponse.ErrorMessage);

                    // PageModel
                    this.Model = interopResponse.Result as TModel;

                    // PageData
                    this.PageData = this.CreatePageData();
                }
            }
            finally
            {
                // Initialized
                this.Initialized = true;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            // Require
            if (firstRender == true)
            {
                // PageLoading
                await this.JSRuntime.InvokeVoidAsync("eval", "mdp.blazor.eventManager.dispatchPageLoading();");

                // return
                return;
            }
            if (this.Initialized == false) return;

            // PageLoaded
            await this.JSRuntime.InvokeVoidAsync("eval", "mdp.blazor.eventManager.dispatchPageLoaded();");
        }

        private object CreatePageData()
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
