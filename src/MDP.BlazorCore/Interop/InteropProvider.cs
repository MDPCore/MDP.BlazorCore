using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public interface InteropProvider
    {
        // Methods
        Task<InteropResponse> InvokeAsync(ClaimsPrincipal principal, InteropRequest interopRequest, InteropResource interopResource);
    }
}
