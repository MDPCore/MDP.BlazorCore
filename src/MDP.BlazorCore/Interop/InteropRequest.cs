using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MDP.BlazorCore
{
    public class InteropRequest
    {
        // Constants
        private readonly static JsonDocument _emptyDocument = JsonDocument.Parse("{}");


        // Constructors
        public InteropRequest(Uri serviceUri, string methodName, JsonDocument methodParameters = null)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(serviceUri);
            ArgumentNullException.ThrowIfNullOrEmpty(methodName);

            #endregion

            // Require
            if (methodParameters == null) methodParameters = _emptyDocument;

            // Default
            this.ServiceUri = serviceUri;
            this.MethodName = methodName;
            this.MethodParameters = methodParameters;

            // RoutePath
            var routePath = this.ServiceUri.AbsolutePath;
            if (routePath.StartsWith("/") == false) routePath = "/" + routePath;
            if (routePath.EndsWith("/") == true) routePath = routePath.TrimEnd('/');
            if (string.IsNullOrEmpty(routePath) == true) throw new InvalidOperationException($"{nameof(routePath)}=null");
            this.RoutePath = routePath;

            // RoutePathSectionList
            var routePathSectionList = routePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (routePathSectionList == null) throw new InvalidOperationException($"{nameof(routePathSectionList)}=null");
            if (routePathSectionList.Count == 0) throw new InvalidOperationException($"{nameof(routePathSectionList)}.Count=0");
            this.RoutePathSectionList = routePathSectionList;
        }


        // Properties
        public Uri ServiceUri { get; private set; }

        public string MethodName { get; private set; }

        public JsonDocument MethodParameters { get; private set; }


        internal string RoutePath { get; private set; }

        internal List<string> RoutePathSectionList { get; private set; }
    }
}
