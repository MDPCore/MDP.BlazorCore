using System.Security.Claims;

namespace MDP.BlazorCore
{
    public abstract class InteropController
    {
        // Properties
        public ClaimsPrincipal User { get; internal set; }


        // Methods

    }
}
