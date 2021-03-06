﻿using Agro.Common;
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
        public const string HOSTNAME = "http://agro-ku.cloudapp.net/";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private User currentUser;

        public PivotPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            GetFeedList();
            if (IsLoggedIn())
            {
                GetNotifications();
                GetDashboard();
                GetName();
            }
        }

        #region Navigation

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
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLoggedIn())
            {
                CommandBar.IsOpen = !CommandBar.IsOpen;
            }
            else
            {
                Frame.Navigate(typeof(LoginPage));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            if (IsLoggedIn())
            {
                NameAtAppBar.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Visible;
            }

            string command = e.Parameter as string;

            if (command.Equals("LoggedIn"))
            {
                GetNotifications();
                GetDashboard();
                GetName();
            }
            Debug.WriteLine("OnNavigatedTo");
        }

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(FeedPage), e.ClickedItem);
        }

        #endregion

        #region SendRequest

        private async void GetFeedList()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                string responseString = await httpClient.GetStringAsync(new Uri(HOSTNAME + "contents.json"));

                var feedList = JsonConvert.DeserializeObject<List<FeedItem>>(responseString);
                FeedListView.IsItemClickEnabled = feedList.Count != 0;
                if (feedList.Count == 0)
                {
                    FeedItem f = new FeedItem();
                    f.Title = resourceLoader.GetString("EmptyFeed");
                    feedList.Add(f);
                }
                FeedListView.ItemsSource = feedList;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fail: GetFeedList()");
                //Debug.WriteLine(ex.ToString());
            }
        }

        private async void GetNotifications()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                string uri = String.Format("{0}notifications.json?username={1}&auth_token={2}", HOSTNAME, localSettings.Values["username"], localSettings.Values["token"]);
                string responseString = await httpClient.GetStringAsync(new Uri(uri));

                List<Notification> notifications = JsonConvert.DeserializeObject<List<Notification>>(responseString);

                bool isItemClickEnabled = notifications.Count != 0;
                if (notifications.Count == 0)
                {
                    Notification n = new Notification();
                    n.DiseaseName = resourceLoader.GetString("EmptyNotification");
                    notifications.Add(n);
                }
                NotificationListViewPage notificationListViewPage = new NotificationListViewPage(notifications, isItemClickEnabled);

                PivotItem pivotItem = new PivotItem();
                pivotItem.Header = resourceLoader.GetString("NotificationPivotHeader");
                pivotItem.Content = notificationListViewPage;
                PivotController.Items.Add(pivotItem);
                pivotItem = null;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fail: GetNotifications()");
                //Debug.WriteLine(ex.ToString());
            }
        }

        private async void GetDashboard()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var dashboards = new List<Dashboard>();
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                string uri = String.Format("{0}cultivated_areas.json?username={1}&auth_token={2}", HOSTNAME, localSettings.Values["username"], localSettings.Values["token"]);
                string responseString = await httpClient.GetStringAsync(new Uri(uri));
                dashboards = JsonConvert.DeserializeObject<List<Dashboard>>(responseString);

                PivotItem pivotItem;
                for (int i = 0; i < dashboards.Count; i++)
                {
                    pivotItem = new PivotItem();
                    pivotItem.Header = resourceLoader.GetString("CultivatedArea") + " " + (i + 1);
                    pivotItem.Content = new DashboardPage(dashboards[i]);
                    PivotController.Items.Add(pivotItem);
                    pivotItem = null;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fail: GetDashboard()");
                //Debug.WriteLine(ex.ToString());
            }

        }

        private async void GetName()
        {
            try
            {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                string uri = String.Format("{0}profile.json?username={1}&auth_token={2}", HOSTNAME, localSettings.Values["username"], localSettings.Values["token"]);
                string responseString = await httpClient.GetStringAsync(new Uri(uri));
                currentUser = JsonConvert.DeserializeObject<User>(responseString);
                NameAtAppBar.Label = currentUser.Name;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Fail: GetFeedList()");
                //Debug.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region User

        private async void ClickToLogOut(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(HOSTNAME + "users/sign_out.json");
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("X-User-Username", localSettings.Values["username"].ToString());
            httpClient.DefaultRequestHeaders.Add("X-Token", localSettings.Values["token"].ToString());
            httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");

            var responseString = await httpClient.DeleteAsync(uri);

            localSettings.Values.Remove("token");
            localSettings.Values.Remove("username");

            NameAtAppBar.Visibility = Visibility.Collapsed;
            LogoutButton.Visibility = Visibility.Collapsed;

            var totalCount = PivotController.Items.Count;
            for (int i = 1; i < totalCount; i++)
            {
                PivotController.Items.RemoveAt(1);
            }
            MessageDialog msgbox = new MessageDialog(resourceLoader.GetString("LogoutMessage"));
            await msgbox.ShowAsync();

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

        private async void OpenUserPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserPage), currentUser);
        }


        #endregion

    }
}
