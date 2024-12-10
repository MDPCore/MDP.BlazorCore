using Microsoft.AspNetCore.Components;
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

        public static string GetQueryValue(this Uri uri, string queryName)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(uri);
            ArgumentNullException.ThrowIfNullOrEmpty(queryName);

            #endregion

            // QueryDictionary
            var queryDictionary = uri.GetQueryDictionary();
            if (queryDictionary == null) throw new InvalidOperationException($"{nameof(queryDictionary)}=null");

            // QueryValue
            if (queryDictionary.ContainsKey(queryName) == true)
            {
                return queryDictionary[queryName];
            }

            // Return
            return null;
        }

        public static Dictionary<string, string> GetQueryDictionary(this Uri uri)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(uri);

            #endregion

            // QueryString
            var queryString = uri.Query;
            if (string.IsNullOrEmpty(queryString) == true) return new Dictionary<string, string>();

            // QueryDictionary
            var queryDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var queryPartList = queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var queryPart in queryPartList)
            {
                var keyValue = queryPart.Split('=', 2);
                var key = keyValue[0];
                var value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : string.Empty;
                queryDictionary[key] = value;
            }

            // Return
            return queryDictionary;
        }
    }
}
