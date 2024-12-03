using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public static class UriExtensions
    {
        // Methods
        public static bool IsInMaui(this Uri uri)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(uri);

            #endregion

            // Android
            if
            (
                uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) == true &&
                uri.Host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase) == true
            ) { return true; }

            // iOS
            if
            (
                uri.Scheme.Equals("app", StringComparison.OrdinalIgnoreCase) == true &&
                uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) == true
            ) { return true; }

            // Other
            return false;
        }
    }
}
