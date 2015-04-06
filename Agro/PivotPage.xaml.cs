using Agro.Common;
using Agro.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http.Headers;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace Agro
{
    public sealed partial class PivotPage : Page
    {
        private const string HostName = "http://obscure-sea-2022.herokuapp.com/";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public PivotPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Debug.WriteLine("NavigationHelper_LoadState");
            GetFeedList();
            GetNotifications();
            GetDashboard();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            Debug.WriteLine("OnNavigatedTo");
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(FeedPage), e.ClickedItem);
        }

        private async void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void GetFeedList()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                string responseString = await httpClient.GetStringAsync(new Uri(HostName + "contents.json"));

                var feedList = JsonConvert.DeserializeObject<List<FeedItem>>(responseString);
                FeedListView.ItemsSource = feedList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fail: GetFeedList()");
                //Debug.WriteLine(ex.ToString());
            }
        }

        private async void GetDashboard()
        {
            if (IsLoggedIn())
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                var dashboards = new List<Dashboard>();
                try
                {
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    string uri = String.Format("{0}cultivated_areas.json?username={1}&auth_token={2}", HostName, localSettings.Values["username"], localSettings.Values["token"]);
                    string responseString = await httpClient.GetStringAsync(new Uri(uri));
                    dashboards = JsonConvert.DeserializeObject<List<Dashboard>>(responseString);

                    PivotItem pvt;
                    for (int i = 0; i < dashboards.Count; i++)
                    {
                        pvt = new PivotItem();
                        pvt.Header = dashboards.ElementAt<Dashboard>(i).Name;
                        var stack = new StackPanel();
                        pvt.Content = stack;
                        pivot.Items.Add(pvt);
                        pvt = null;
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fail: GetDashboard()");
                    //Debug.WriteLine(ex.ToString());
                }
            }

        }


        private async void GetNotifications()
        {
            if (IsLoggedIn())
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                try
                {
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    string uri = String.Format("{0}notifications.json?username={1}&auth_token={2}", HostName, localSettings.Values["username"], localSettings.Values["token"]);
                    string responseString = await httpClient.GetStringAsync(new Uri(uri));

                    var notifications = JsonConvert.DeserializeObject<List<Notification>>(responseString);
                    NotificationListView.ItemsSource = notifications;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fail: GetNotifications()");
                    //Debug.WriteLine(ex.ToString());
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void ClickToLogin(object sender, RoutedEventArgs e)
        {
            string username = UsernameField.Text;
            var password = PasswordField.Password;

            PasswordField.Password = "";
            UsernameField.Text = "";

            var uri = new Uri(HostName + "users/sign_in");
            var json = "{\"user\": {\"username\" :\"" + username + "\", \"password\": \"" + password + "\" }}";

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            var responseString = await httpClient.PostAsync(uri, new HttpStringContent(json, UnicodeEncoding.Utf8,
                                    "application/json"));
            string token = responseString.Content.ToString();
            Debug.WriteLine(token);
            KeepData(token, username);

            MessageBoxDisplay("ยินดีต้อนรับเข้าสู่ระบบ");

            LoginPanel.Visibility = Visibility.Collapsed;
            UserProFilePanel.Visibility = Visibility.Visible;

            GetNotifications();
            GetDashboard();
        }

        private async void ClickToLogOut(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(HostName + "users/sign_out.json");
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("X-User-Username", localSettings.Values["username"].ToString());
            httpClient.DefaultRequestHeaders.Add("X-Token", localSettings.Values["token"].ToString());
            httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");

            var responseString = await httpClient.DeleteAsync(uri);

            localSettings.Values.Remove("token");
            localSettings.Values.Remove("username");

            MessageBoxDisplay("คุณได้ออกจากระบบแล้ว");

            LoginPanel.Visibility = Visibility.Visible;
            UserProFilePanel.Visibility = Visibility.Collapsed;
        }

        private bool IsLoggedIn()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var isLoggedIn = localSettings.Values.ContainsKey("token") && localSettings.Values.ContainsKey("username");
            if (isLoggedIn)
            {
                Debug.WriteLine(String.Format("Logged in as {0}: {1}", localSettings.Values["username"], localSettings.Values["token"]));
            }
            return isLoggedIn;
        }

        private void KeepData(string token, string username)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (!IsLoggedIn())
            {
                localSettings.Values.Add("token", token);
                localSettings.Values.Add("username", username);
            }
            return;
        }

        private async void MessageBoxDisplay(string message)
        {
            MessageDialog msgbox = new MessageDialog(message);
            await msgbox.ShowAsync();
        }

    }
}
