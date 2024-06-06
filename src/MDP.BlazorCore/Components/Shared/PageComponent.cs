using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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
        // Constructors
        protected override async Task OnInitializedAsync()
        {
            // Base
            await base.OnInitializedAsync();

            // NavigationUri
            var navigationUri = new Uri(this.NavigationManager.Uri);
            if (navigationUri == null) throw new InvalidOperationException($"{nameof(navigationUri)}=null");

            // ServiceUri
            var serviceUri = $"{navigationUri.Scheme}://{navigationUri.Host}{navigationUri.AbsolutePath}";
            if (serviceUri == null) throw new InvalidOperationException($"{nameof(serviceUri)}=null");

            // Principal
            var principal = (await this.AuthenticationStateProvider.GetAuthenticationStateAsync())?.User;
            if (principal == null) throw new InvalidOperationException($"{nameof(principal)}=null");

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
                var interopResponse = await this.InteropManager.InvokeAsync(new InteropRequest
                (
                    new Uri(serviceUri),
                    nameof(OnInitializedAsync)
                ), principal, this.ServiceProvider);
                if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");
                if (interopResponse.Succeeded == false) throw new InvalidOperationException(interopResponse.ErrorMessage);

                // PageModel
                this.Model = interopResponse.Result as TModel;
            }
        }


        // Properties
        [Parameter]
        public TModel Model { get; set; }

        [Inject]
        public InteropManager InteropManager { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public AuthenticationStateProvider AuthenticationStateProvider { get; set; }


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
