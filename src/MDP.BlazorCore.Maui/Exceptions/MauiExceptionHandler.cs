using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MDP.BlazorCore.Maui
{
    internal static class MauiExceptionHandler
    {
        // Methods
        public static void Initialize()
        {
            // UnhandledException
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                // Handle
                Handle(e.ExceptionObject as Exception);
            };

            // UnobservedTaskException
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                // Handle
                Handle(e.Exception);

                // Return
                e.SetObserved(); 
            };
        }

        public static void Handle(Exception exception)
        {
            // Require
            if (exception == null) return;

            // Log
            Console.WriteLine($"[ERROR] FATAL UNHANDLED EXCEPTION: {exception.GetType()}: {exception.Message} \n{exception.StackTrace}");

            // Display
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    await Application.Current.MainPage.DisplayAlert(
            //        "系統發生錯誤，請稍後再試或聯絡系統管理員",
            //        $"[ERROR] FATAL UNHANDLED EXCEPTION: {exception.GetType()}: {exception.Message} \n{exception.StackTrace}",
            //        "確定"
            //    );
            //});
        }
    }
}
