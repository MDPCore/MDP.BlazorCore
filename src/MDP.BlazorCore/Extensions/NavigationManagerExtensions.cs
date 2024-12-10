using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace MDP.BlazorCore
{
    public static class NavigationManagerExtensions
    {
        // Methods
        public static Dictionary<string, string> GetQueryDictionary(this NavigationManager navigationManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(navigationManager);

            #endregion

            // Return
            return (new Uri(navigationManager.Uri)).GetQueryDictionary();
        }

        public static string GetQueryValue(this NavigationManager navigationManager, string queryName)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(navigationManager);
            ArgumentNullException.ThrowIfNullOrEmpty(queryName);

            #endregion

            // Return
            return (new Uri(navigationManager.Uri)).GetQueryValue(queryName);
        }


        public static void NavigateToLogin(this NavigationManager navigationManager, RoutesOptions routesOptions)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(navigationManager);
            ArgumentNullException.ThrowIfNull(routesOptions);

            #endregion

            // NavigateTo
            navigationManager.NavigateToPath(routesOptions.LoginPath);
        }

        public static void NavigateToLogout(this NavigationManager navigationManager, RoutesOptions routesOptions)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(navigationManager);
            ArgumentNullException.ThrowIfNull(routesOptions);

            #endregion

            // NavigateTo
            navigationManager.NavigateToPath(routesOptions.LogoutPath);
        }

        public static void NavigateToAccessDenied(this NavigationManager navigationManager, RoutesOptions routesOptions)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(navigationManager);
            ArgumentNullException.ThrowIfNull(routesOptions);

            #endregion

            // NavigateTo
            navigationManager.NavigateToPath(routesOptions.AccessDeniedPath);
        }

        private static void NavigateToPath(this NavigationManager navigationManager, string path)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(navigationManager);
            ArgumentNullException.ThrowIfNullOrEmpty(path);

            #endregion

            // ReturnUrl
            var returnUrl = string.Empty;
            if (string.IsNullOrEmpty(returnUrl) == true) returnUrl = (new Uri(navigationManager.Uri)).PathAndQuery;
            if (string.IsNullOrEmpty(returnUrl) == true) returnUrl = "/";

            // RedirectUri
            var redirectUri = path;
            if (string.IsNullOrEmpty(redirectUri) == true) throw new InvalidOperationException($"{nameof(redirectUri)}=null");
            if (redirectUri.StartsWith("/") == false) redirectUri = "/" + redirectUri;
            if (redirectUri.EndsWith("/") == true) redirectUri = redirectUri.TrimEnd('/');

            // NavigateTo
            navigationManager.NavigateTo($"{redirectUri}?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
        }
    }
}
