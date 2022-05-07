using PulsedApp.Services;
using PulsedApp.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace PulsedApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public string wow = "wowowow";
        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            // TODO: Create command that gets auth token and stores it.
            LoginCommand = new Command(OnLoginClicked);
        }

        private async void OnLoginClicked(object obj)
        {
            // Prefixing with `//` switches to a different navigation stack instead of pushing to the active one
            await Shell.Current.GoToAsync($"//{nameof(AboutPage)}");
        }
        
    }
}
