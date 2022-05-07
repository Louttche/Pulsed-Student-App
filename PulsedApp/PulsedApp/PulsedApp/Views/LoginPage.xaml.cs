using PulsedApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PulsedApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public WebView view;

        // URL properties for Fontys authentication
        string CLIENT_ID = "i874073-no-production";
        string REDIRECT_URI = "https://tas.fhict.nl/oob.html";
        string OAUTH_URL = "https://identity.fhict.nl/connect/authorize";
        string OAUTH_SCOPE = "fhict fhict_personal";

        public LoginPage()
        {
            InitializeComponent();
            this.BindingContext = new LoginViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            view = new WebView();

            Content = new StackLayout
            {
                Children = { view }
            };

            view.Navigated += View_Navigated;
            view.Source = "http://google.com";

        }

        private void View_Navigated(object sender, WebNavigatedEventArgs e)
        {
            (sender as WebView).HeightRequest = Height;
        }
    }
}