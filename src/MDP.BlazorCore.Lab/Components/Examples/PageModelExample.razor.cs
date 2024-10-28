using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Components.Examples
{
    public partial class PageModelExample
    {
        // Controller
        public class PageController : InteropController
        {
            // Methods
            public async Task<PageModel> OnInitializedAsync()
            {
                // Return
                return await Task.FromResult(new PageModel
                {
                    Message = "Hello World"
                });
            }
        }


        // Model
        public class PageModel
        {
            // Properties
            public string Message { get; set; }
        }
    }
}
