using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class InteropRouteAttribute : Attribute
    {
        // Constructors
        public InteropRouteAttribute(string template)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNullOrEmpty(template);

            #endregion

            // Default
            this.Template = template;
        }


        // Properties
        public string Template { get; }
    }
}
