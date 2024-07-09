using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class PageComponent : ComponentBase
    {
        // Constructors
        protected override async Task OnInitializedAsync()
        {
            // Base
            await Task.Yield();
            await base.OnInitializedAsync();
        }


        // Properties
        [Inject]
        public InteropManager InteropManager { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    }

    public class PageComponent<TModel> : PageComponent where TModel : class, new()
    {
        // Constructors
        protected override async Task OnInitializedAsync()
        {
            // Base
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
            }
        }


        // Properties
        [Parameter]
        public TModel Model { get; set; }


        // Methods
        protected object CreatePageData()
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
