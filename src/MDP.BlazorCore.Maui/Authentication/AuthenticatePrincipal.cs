using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class AuthenticatePrincipal
    {
        // Constructors
        public AuthenticatePrincipal() { }

        public AuthenticatePrincipal(ClaimsPrincipal claimsPrincipal, DateTime expireTime) 
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(claimsPrincipal);

            #endregion

            // ClaimsIdentity
            var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
            if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

            // ClaimList
            var claimList = claimsIdentity.Claims.ToList();
            claimList.RemoveAll(o => o.Type == AuthenticationClaimTypes.AuthenticationType);
            if (claimsIdentity.IsAuthenticated == true)
            {
                // AuthenticationType
                claimList.Add(new Claim(AuthenticationClaimTypes.AuthenticationType, claimsIdentity.AuthenticationType));
            }

            // Default
            this.ClaimList = claimList;
            this.ExpireTime = expireTime;
        }


        // Properties
        public List<Claim> ClaimList { get; set; }

        public DateTime ExpireTime { get; set; }


        // Methods
        public ClaimsPrincipal CreateClaimsPrincipal()
        {
            // AuthenticationType
            var authenticationType = this.ClaimList.FirstOrDefault(o => o.Type == AuthenticationClaimTypes.AuthenticationType)?.Value;
            if (string.IsNullOrEmpty(authenticationType) == true)
            {
                // Return
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            // ClaimList
            var claimList = this.ClaimList.Where(o => o.Type != AuthenticationClaimTypes.AuthenticationType).ToList();
            if (claimList == null) throw new InvalidOperationException($"{nameof(claimList)}=null");

            // Return
            return new ClaimsPrincipal(new ClaimsIdentity(claimList, authenticationType));
        }
    }
}
