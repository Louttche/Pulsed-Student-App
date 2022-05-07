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
        string token = "";
        public string Token { get => token; } // TODO: set token when getting it from fontys api

        public Command LoginCommand { get; }

        public LoginViewModel()
        {
            Title = "Fontys Auth";
            // TODO: Create command that gets auth token and stores it.
        }        
    }
}
