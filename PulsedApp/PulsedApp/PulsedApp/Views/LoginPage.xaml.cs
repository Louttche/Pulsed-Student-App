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
        // URL properties for Fontys authentication
        string CLIENT_ID = "i874073-no-production";
        string REDIRECT_URI = "https://tas.fhict.nl/oob.html";
        string OAUTH_URL = "https://identity.fhict.nl/connect/authorize";
        string OAUTH_SCOPE = "fhict fhict_personal";

        // TODO: store it more securely (in separate script)
        string auth_token;

        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            string full_uri = OAUTH_URL + "?redirect_uri=" + REDIRECT_URI + "&response_type=token&client_id=" + CLIENT_ID + "&scope=" + OAUTH_SCOPE;

            fontysLoginWebView.Navigated += CheckForToken;
            fontysLoginWebView.HorizontalOptions = LayoutOptions.End;
            fontysLoginWebView.Source = full_uri;

            Label lbl = new Label();
            lbl.Text = "Fontys Authentication";
            lbl.HorizontalOptions = LayoutOptions.Start;
        }

        private void CheckForToken(object sender, WebNavigatedEventArgs e)
        {
            if (e.Url.Contains("access_token="))
            {
                auth_token = e.Url.Split(new string[] { "#access_token=" }, StringSplitOptions.None)[1].Split('&')[0];
                //Console.WriteLine("Token found: " + auth_token);
            }
        }
    }
}