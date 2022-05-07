using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PulsedApp.ViewModels
{
    public class EquipmentViewModel : BaseViewModel
    {
        public EquipmentViewModel()
        {
            Title = "Map";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
    }
}