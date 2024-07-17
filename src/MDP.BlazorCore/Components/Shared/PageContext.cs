using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class PageContext
    {
        // Constructors
        public PageContext(Dictionary<string, object> pageData = null, Exception pageError = null)
        {
            // Default
            this.PageData = pageData ?? new Dictionary<string, object>();
            this.PageError = pageError; 
        }


        // Properties
        public Dictionary<string, object> PageData { get; private set; } = null;

        public Exception PageError { get; private set; } = null;
    }
}
