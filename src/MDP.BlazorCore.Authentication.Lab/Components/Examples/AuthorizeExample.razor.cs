using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Components.Examples
{
    public partial class AuthorizeExample
    {
        // Controller
        public class PageController : InteropController
        {
            // Methods
            public Task OnInitializedAsync()
            {
                // Return
                return Task.CompletedTask;
            }

            public async Task<EchoResultModel> Echo(string message)
            {
                // Return
                return await Task.FromResult(new EchoResultModel
                {
                    Message = "Hello World"
                });
            }
        }


        // Model
        public class EchoResultModel
        {
            // Properties
            public string Message { get; set; }
        }
    }
}
