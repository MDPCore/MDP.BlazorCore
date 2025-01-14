﻿using MDP.Configuration;
using MDP.Hosting;
using MDP.NetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using System;

namespace MDP.BlazorCore.Maui
{
    internal static class MauiApplicationBuilderExtensions
    {
        // Methods
        public static MauiAppBuilder ConfigureMDP<TProgram>(this MauiAppBuilder applicationBuilder, Type defaultLayout = null) where TProgram : class
        {
            #region Contracts

            if (applicationBuilder == null) throw new ArgumentException($"{nameof(applicationBuilder)}=null");

            #endregion

            // Initialize
            MauiExceptionHandler.Initialize();
            MauiReferenceAssemblies.Initialize();

            // EntryAssembly
            var entryAssembly = typeof(TProgram).Assembly;
            if (entryAssembly == null) throw new InvalidOperationException($"{nameof(entryAssembly)}=null");

            // HostEnvironment
            var hostEnvironment = new MauiHostEnvironment(new MauiEnvironmentVariables(), entryAssembly);
            if (hostEnvironment == null) throw new InvalidOperationException($"{nameof(hostEnvironment)}=null");

            // ConfigurationBuilder
            var configurationBuilder = applicationBuilder.Configuration;
            {
                // ConfigurationRegister
                ConfigurationRegister.RegisterModule(configurationBuilder, new MDP.BlazorCore.Maui.MauiConfigurationProvider(hostEnvironment.EnvironmentName));
            }

            // ContainerBuilder
            var serviceCollection = applicationBuilder.Services;
            {
                // ContainerRegister
                {
                    ServiceFactoryRegister.RegisterModule(applicationBuilder, applicationBuilder.Configuration);
                }
                ContainerRegister.RegisterModule(serviceCollection, applicationBuilder.Configuration);

                // HostEnvironment
                serviceCollection.AddSingleton<IHostEnvironment>(hostEnvironment);
            }

            // BlazorBuilder
            {
                // DeveloperTools
                if (hostEnvironment.IsDevelopment() == true)
                {
                    applicationBuilder.Services.AddBlazorWebViewDeveloperTools();
                    applicationBuilder.Logging.AddDebug();
                }

                // BlazorApp
                applicationBuilder.UseMauiApp<MDP.BlazorCore.Maui.App>();
                applicationBuilder.Services.AddMauiBlazorWebView();
                applicationBuilder.Services.AddAuthorizationCore();

                // Routes
                applicationBuilder.Services.AddSingleton<RoutesOptions>(serviceProvider => { 
                    return new RoutesOptions()
                    {
                        AppAssembly = entryAssembly,
                        DefaultLayout = defaultLayout
                    };
                });

                // ActivationManager
                applicationBuilder.Services.AddSingleton<ActivationManager>();

                // AuthenticationManager
                applicationBuilder.Services.AddSingleton<AuthenticationManager>();

                // AuthenticationTokenManager
                applicationBuilder.Services.AddSingleton<AuthenticationTokenManager>();

                // AuthenticationStateManager
                applicationBuilder.Services.AddSingleton<AuthenticationStateManager>();

                // AuthenticationStateProvider
                applicationBuilder.Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, MDP.BlazorCore.Maui.AuthenticationStateProvider>();

                // AuthorizationManager
                applicationBuilder.Services.AddSingleton<AuthorizationManager>();

                // InteropManager
                applicationBuilder.Services.AddInteropManager();

                // InteropProvider
                applicationBuilder.Services.AddSingleton<InteropProvider, RemoteInteropProvider>();                
            }

            // Return
            return applicationBuilder;
        }
    }
}
