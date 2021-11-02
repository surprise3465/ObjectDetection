using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ObjectDetection
{
    public interface IReadWritePermission
    {
        Task<PermissionStatus> CheckStatusAsync();
        Task<PermissionStatus> RequestAsync();
    }

    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Plugin.Media.CrossMedia.Current.Initialize();    
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
           
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
