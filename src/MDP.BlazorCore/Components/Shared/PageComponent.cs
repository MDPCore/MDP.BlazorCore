using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class PageComponent : PageComponent<PageComponent<object>.EmptyPageModel>
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

            // PageModel
            if (typeof(TModel) != typeof(PageComponent<object>.EmptyPageModel))
            {
                // OnInitializedAsync
                var interopResponse = await this.InteropManager?.InvokeAsync(new InteropRequest
                (
                    new Uri(serviceUri),
                    nameof(OnInitializedAsync)
                ), this.ServiceProvider);
                if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");
                if (interopResponse.Succeeded == false) throw new InvalidOperationException(interopResponse.ErrorMessage);

                // Setup
                this.Model = interopResponse.Result as TModel;
            }
        }


        // Properties
        public TModel Model { get; set; }

        [Inject]
        public InteropManager InteropManager { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }


        // Methods
        public object CreatePageData()
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


        // Class
        public class EmptyPageModel
        {

        }
    }
}
