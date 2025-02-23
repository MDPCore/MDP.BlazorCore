﻿using MDP.BlazorCore.Components;
using MDP.BlazorCore.Components.Layout;

namespace MDP.BlazorCore.Maui.Lab
{
    public class MauiProgram
    {
        // Methods
        public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
        {
            // Host    
            return MDP.BlazorCore.Maui.Host.CreateMauiApp<MauiProgram>(typeof(MainLayout));        
        }          
    }       
}                                                                                 
                                                                                                