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
        public InteropRequest(Uri controllerUri, string actionName, JsonDocument actionParameters = null)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(controllerUri);
            ArgumentNullException.ThrowIfNullOrEmpty(actionName);

            #endregion

            // Require
            if (actionParameters == null) actionParameters = _emptyDocument;

            // Default
            this.ControllerUri = controllerUri;
            this.ActionName = actionName;
            this.ActionParameters = actionParameters;

            // RoutePath
            var routePath = this.ControllerUri.AbsolutePath;
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
        public Uri ControllerUri { get; private set; }

        public string ActionName { get; private set; }

        public JsonDocument ActionParameters { get; private set; }


        internal string RoutePath { get; private set; }

        internal List<string> RoutePathSectionList { get; private set; }
    }
}
