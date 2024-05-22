using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MDP.BlazorCore.Lab.UserHandler;

namespace MDP.BlazorCore.Lab
{
    public class UserHandler : InteropHandler
    {
        // Methods
        [AllowAnonymous]
        [InteropRoute("/[TenantId]/User/FindByTenantId")]
        public async Task<User> FindByTenantId(string tenantId, User user)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNullOrEmpty(tenantId);
            ArgumentNullException.ThrowIfNull(user);

            #endregion

            // Setting
            user.TenantId = tenantId;

            // Return
            return await Task.FromResult(user);
        }

        public class User
        {
            // Properties
            public string TenantId { get; set; } = null;

            public string UserId { get; set; } = null;

            public string Name { get; set; } = null;
        }
    }
}
