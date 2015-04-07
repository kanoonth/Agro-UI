using Agro.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace Agro
{

    public sealed partial class LoginPage : Page
    {
        private ResourceLoader Resource;

        public LoginPage()
        {
            this.InitializeComponent();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            Resource = new ResourceLoader();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
        }

        private async void ClickToLogin(object sender, RoutedEventArgs e)
        {
            string username = UsernameField.Text;
            var password = PasswordField.Password;

            PasswordField.Password = "";
            UsernameField.Text = "";

            var uri = new Uri(PivotPage.HOSTNAME + "users/sign_in");
            var json = "{\"user\": {\"username\" :\"" + username + "\", \"password\": \"" + password + "\" }}";

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            var responseString = await httpClient.PostAsync(uri, new HttpStringContent(json, UnicodeEncoding.Utf8,
                                    "application/json"));
            string token = responseString.Content.ToString();

            string loginResult = responseString.StatusCode.ToString();
            if (loginResult.Equals("Ok"))
            {
                KeepData(token, username);
                string greetingLogin = Resource.GetString("GreetingLogin");
                MessageBoxDisplay(greetingLogin);
                Frame.Navigate(typeof(PivotPage),"LoggedIn");
            }
            else if (loginResult.Equals("Unauthorized"))
            {
                string InvalidLogin = Resource.GetString("InvalidLogin");
                MessageBoxDisplay(InvalidLogin);
                UsernameField.Text = "";
                PasswordField.Password = "";
            }
            else
            {
                Debug.WriteLine(responseString.StatusCode);
            }
        }


        private void KeepData(string token, string username)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values.Add("token", token);
                localSettings.Values.Add("username", username);
        }
        
        private async void MessageBoxDisplay(string message)
        {
            MessageDialog msgbox = new MessageDialog(message);
            await msgbox.ShowAsync();
        }
    }
}
