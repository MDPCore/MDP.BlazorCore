using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropResponse
    {
        // Properties
        public bool Succeeded { get; set; }

        public object Result { get; set; }

        public string ErrorMessage { get; set; }
    }
}
