using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authorization.Components.Examples
{
    public partial class PageControllerExample
    {
        // Controller
        public class PageController : InteropController
        {
            // Methods
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
