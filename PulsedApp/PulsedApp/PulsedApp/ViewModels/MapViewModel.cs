using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PulsedApp.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        //public ICommand OpenWebCommand { get; }
        public string Image { get => "pulsed_map.png"; }

        public MapViewModel()
        {
            Title = "Map";
            //OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }
    }
}