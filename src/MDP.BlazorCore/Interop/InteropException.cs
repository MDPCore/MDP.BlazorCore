using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropException : Exception
    {
        // Constructors
        public InteropException(string message) : base(message) { }

        public InteropException(string message, Exception innerException) : base(message, innerException) { }
    }
}
