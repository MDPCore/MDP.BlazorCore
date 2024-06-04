﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MDP.Registration;
using Microsoft.AspNetCore.Components.Authorization;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class MauiAuthenticationStateProvider : AuthenticationStateProvider
    {
        // Fields
        private AuthenticationStateManager _authenticationStateManager;


        // Constructors
        public MauiAuthenticationStateProvider(AuthenticationStateManager authenticationStateManager)
        {
            #region Contracts

            if (authenticationStateManager == null) throw new ArgumentException($"{nameof(authenticationStateManager)}=null");

            #endregion

            // Default
            _authenticationStateManager = authenticationStateManager;

            // Event
            authenticationStateManager.PrincipalChanged += this.AuthenticationStateManager_PrincipalChanged;
        }


        // Methods
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // ClaimsPrincipal
            var claimsPrincipal = await _authenticationStateManager.GetPrincipalAsync();
            if(claimsPrincipal == null) throw new InvalidOperationException($"{nameof(claimsPrincipal)}=null");

            // Return
            return new AuthenticationState(claimsPrincipal);
        }


        // Handlers
        private void AuthenticationStateManager_PrincipalChanged(ClaimsPrincipal claimsPrincipal)
        {
            #region Contracts

            if (claimsPrincipal == null) throw new ArgumentException($"{nameof(claimsPrincipal)}=null");

            #endregion

            // Notify
            this.NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}