using Microsoft.Maui.Controls;
using System.Reflection;
using System;

namespace MDP.BlazorCore.Maui
{
    public partial class App : Application
    {
        // Constructors
        public App()
        {
            // Initialize
            this.InitializeComponent();

            // MainPage
            this.MainPage = new MDP.BlazorCore.Maui.AppPage();
        }
    }
}
