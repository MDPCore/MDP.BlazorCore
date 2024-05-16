using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authorization.Maui
{
    public interface IAccessResourceProvider
    {
        // Methods
        AccessResource Create();
    }
}
