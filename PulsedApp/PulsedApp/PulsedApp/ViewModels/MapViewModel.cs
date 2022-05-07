using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PulsedApp.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        public MapViewModel()
        {
            Title = "Map";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
    }
}