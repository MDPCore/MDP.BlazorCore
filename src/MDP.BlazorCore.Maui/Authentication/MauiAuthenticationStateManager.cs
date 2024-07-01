using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json.Serialization;
using System.Text.Json;
using GoogleGson;
using Java.Lang;

namespace MDP.BlazorCore.Maui
{
    internal class MauiAuthenticationStateManager : AuthenticationStateManager
    {
        // Methods
        public override async Task SetAsync(ClaimsPrincipal claimsPrincipal)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(claimsPrincipal);

            #endregion

            // ClaimList
            var claimList = claimsPrincipal.Claims.Select(claim => new KeyValuePair<string, string>(claim.Type, claim.Value)).ToList();
            claimList = claimList.Where(claim => claim.Key != AuthenticationClaimTypes.AuthenticationType).ToList();
            claimList.Add(new KeyValuePair<string, string>(AuthenticationClaimTypes.AuthenticationType, claimsPrincipal.Identity.AuthenticationType));

            // ClaimListString
            var claimListString = JsonSerializer.Serialize(claimList);
            if (string.IsNullOrEmpty(claimListString) == true) throw new InvalidOperationException($"{nameof(claimListString)}=null");

            // Set
            await SecureStorage.SetAsync(this.GetType().FullName, claimListString);

            // Raise
            this.OnPrincipalChanged(claimsPrincipal);
        }

        public override async Task<ClaimsPrincipal> GetAsync()
        {
            // ClaimListString
            var claimListString = await SecureStorage.GetAsync(this.GetType().FullName);
            if (string.IsNullOrEmpty(claimListString) == true) return null;

            // ClaimList
            var claimList = JsonSerializer.Deserialize<List<KeyValuePair<string, string>>>(claimListString);
            if (claimList == null) throw new InvalidOperationException($"{nameof(claimList)}=null");
           
            // AuthenticationType
            var authenticationType = claimList.FirstOrDefault(o => o.Key == AuthenticationClaimTypes.AuthenticationType).Value as string;
            if (string.IsNullOrEmpty(authenticationType) == true) throw new InvalidOperationException($"{nameof(authenticationType)}=null");

            // Return
            return new ClaimsIdentity(claimList, authenticationType);
        }        

        public override Task ClearAsync()
        {
            // CurrentPrincipal
            var currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            _currentPrincipal = currentPrincipal;

            // Raise
            this.OnPrincipalChanged(currentPrincipal);

            // Return
            return Task.CompletedTask;
        }
    }
}
