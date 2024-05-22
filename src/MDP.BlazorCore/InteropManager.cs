using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore
{
    public class InteropManager
    {
        // Fields
        private readonly Dictionary<string, InteropMethod> _interopMethodDictionary = null;

        private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider = null;

        private AuthorizationPolicy _authorizationPolicy = null;


        // Constructors
        public InteropManager(Dictionary<string, InteropMethod> interopMethodDictionary, IAuthorizationPolicyProvider authorizationPolicyProvider)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopMethodDictionary);
            ArgumentNullException.ThrowIfNull(authorizationPolicyProvider);

            #endregion

            // Default
            _interopMethodDictionary = interopMethodDictionary;
            _authorizationPolicyProvider = authorizationPolicyProvider;
        }


        // Methods
        public async Task<object> InvokeAsync(InteropRequest interopRequest)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(interopRequest);

            #endregion

            // Path
            var path = interopRequest.Uri.AbsolutePath;
            if (path.StartsWith("/") == false) path = "/" + path;
            if (path.EndsWith("/") == true) path = path.TrimEnd('/');
            if (string.IsNullOrEmpty(path) == true) throw new InvalidOperationException($"{nameof(path)}=null");

            // PathSectionList
            var pathSectionList = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathSectionList == null) throw new InvalidOperationException($"{nameof(pathSectionList)}=null");
            if (pathSectionList.Count == 0) throw new InvalidOperationException($"{nameof(pathSectionList)}.Count=0");

            // InteropMethod
            InteropMethod interopMethod = null;
            if (interopMethod == null) interopMethod = this.FindInteropMethod(path);
            if (interopMethod == null) interopMethod = this.FindInteropMethod(pathSectionList);
            if (interopMethod == null) throw new InvalidOperationException($"{nameof(interopMethod)}=null");

            // AuthorizationService
            var authorizationService = interopRequest.ServiceProvider.GetService<IAuthorizationService>();
            if (authorizationService == null) throw new InvalidOperationException($"{nameof(authorizationService)}=null");

            // AuthorizationPolicy
            var authorizationPolicy = await this.CreateAuthorizationPolicyAsync();
            if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");

            // AuthorizationResult
            var authorizationResult = await authorizationService.AuthorizeAsync(interopRequest.User, interopRequest.Resource, authorizationPolicy);
            if (authorizationResult.Succeeded == false) throw new UnauthorizedAccessException($"Authorization failed for resource '{interopRequest.Uri.ToString()}'");

            // InvokeAsync
            return await interopMethod.InvokeAsync(pathSectionList, interopRequest.Payload, interopRequest.ServiceProvider);
        }


        private async Task<AuthorizationPolicy> CreateAuthorizationPolicyAsync()
        {
            // Require
            if (_authorizationPolicy != null) return _authorizationPolicy;

            // Create
            var authorizationPolicy = await _authorizationPolicyProvider.GetDefaultPolicyAsync();
            if (authorizationPolicy == null) throw new InvalidOperationException($"{nameof(authorizationPolicy)}=null");
            _authorizationPolicy = authorizationPolicy;

            // Return
            return _authorizationPolicy;
        }

        private InteropMethod FindInteropMethod(string path)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNullOrEmpty(path);

            #endregion

            // Result
            InteropMethod interopMethod = null;

            // FindByPath
            if (_interopMethodDictionary.TryGetValue(path, out interopMethod) == true) return interopMethod;

            // Return
            return null;
        }

        private InteropMethod FindInteropMethod(List<string> pathSectionList)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(pathSectionList);

            #endregion

            // Result
            InteropMethod interopMethod = null;

            // FindByPathSectionList
            interopMethod = _interopMethodDictionary.Values.FirstOrDefault(interopMethod => interopMethod.CanInvoke(pathSectionList) == true);
            if (interopMethod != null) return interopMethod;

            // Return
            return null;
        }
    }
}
