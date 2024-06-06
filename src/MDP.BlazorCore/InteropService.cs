using System.Security.Claims;

namespace MDP.BlazorCore
{
    public abstract class InteropService
    {
        // Properties
        public ClaimsPrincipal User { get; internal set; }


        // Methods
    }
}
