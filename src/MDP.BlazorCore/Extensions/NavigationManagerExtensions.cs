using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Web;
using System.Xml.Linq;

namespace MDP.BlazorCore
{
    public static class NavigationManagerExtensions
    {
        // Methods
        public static Dictionary<string, string> ParseQuery(this NavigationManager navigationManager)
        {
            #region Contracts

            if (navigationManager == null) throw new ArgumentException($"{nameof(navigationManager)}=null");

            #endregion

            // QueryString
            var queryString = new Uri(navigationManager.Uri).Query;
            if (string.IsNullOrEmpty(queryString)==true) return new Dictionary<string, string>();

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
