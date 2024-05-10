using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication
{
    public interface IAuthorizationProviderBuilder
    {
        // Properties
        string Name { get; }


        // Methods
        IAuthorizationProvider BuildProvider(IServiceProvider serviceProvider);
    }
}
