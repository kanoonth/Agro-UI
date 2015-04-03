using Agro.Common;
using Agro.Data;
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

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace Agro
{
    public sealed partial class PivotPage : Page
    {
        private const string FirstGroupName = "FirstGroup";
        private const string SecondGroupName = "SecondGroup";

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

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-1");
            this.DefaultViewModel[FirstGroupName] = sampleDataGroup;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Adds an item to the list when the app bar button is clicked.
        /// </summary>
        private void AddAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            string groupName = this.pivot.SelectedIndex == 0 ? FirstGroupName : SecondGroupName;
            var group = this.DefaultViewModel[groupName] as SampleDataGroup;
            var nextItemId = group.Items.Count + 1;
            var newItem = new SampleDataItem(
                string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
                string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
                string.Empty,
                string.Empty,
                this.resourceLoader.GetString("NewItemDescription"),
                string.Empty);

            group.Items.Add(newItem);

            // Scroll the new item into view.
            var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
            var listView = container.ContentTemplateRoot as ListView;
            listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter


            Frame.Navigate(typeof(FeedPage),e.ClickedItem);
            //var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            //if (!Frame.Navigate(typeof(ItemPage), itemId))
            //{
            //    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            //}
        }

        /// <summary>
        /// Loads the content for the second pivot item when it is scrolled into view.
        /// </summary>
        private async void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-2");
            this.DefaultViewModel[SecondGroupName] = sampleDataGroup;
        }

        private async void GetFeedList()
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");

                string ResponseString = await httpClient.GetStringAsync(
                                            new Uri("http://obscure-sea-2022.herokuapp.com/contents.json"));

                var feedList = JsonConvert.DeserializeObject<List<FeedItem>>(ResponseString);

                foreach(FeedItem f in feedList) {
                    string thumbURL = String.Format("http://obscure-sea-2022.herokuapp.com{0}",f.ThumbURL);
                    string imgURL = String.Format("http://obscure-sea-2022.herokuapp.com{0}", f.ImageURL);
                    f.ThumbURL = thumbURL;
                    f.ImageURL = imgURL;
                }

                FeedListView.ItemsSource = feedList;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            GetFeedList();
            //GetNotificationsIfLoggedIn();
            //GetDashboardIfLoggedIn();s

            PivotItem pvt;
            for (int i = 0; i < 5; i++)
            {
                pvt = new PivotItem();
                pvt.Header = "แปลงปลูกที่ " + i;
                var stack = new StackPanel();
                pvt.Content = stack;
                pivot.Items.Add(pvt);
                pvt = null;
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(pivot.SelectedIndex);
            
            //int index = pivot.SelectedIndex;
            //if (index == 1)
            //{
            //    Debug.WriteLine(index == 1);
            //    pivot.SelectedIndex = 2;
            //}

        } 


        private async void GetDashboardIfLoggedIn()
        {

            var dashboards = new List<Dashboard>();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (IsLogin())
            {
                try
                {
                    HttpClient httpClient = new HttpClient();

                    httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    string uri = String.Format("http://obscure-sea-2022.herokuapp.com/Dashboard.json?username={0}&auth_token={1}", localSettings.Values["username"], localSettings.Values["token"]);
                    Debug.WriteLine(uri);
                    string ResponseString = await httpClient.GetStringAsync(
                                                new Uri(uri));


                    Debug.WriteLine(ResponseString);
                    dashboards = JsonConvert.DeserializeObject<List<Dashboard>>(ResponseString);

                    // NotificationListView.ItemsSource = notifications;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            
            
            PivotItem pvt;
            for (int i = 0; i < dashboards.Count; i++)
            {
                pvt = new PivotItem();
                pvt.Header = "แปลงปลูกที่ " + i;
                var stack = new StackPanel();
                pvt.Content = stack;
                pivot.Items.Add(pvt);
                pvt = null;
            }


        }


        private async void GetNotificationsIfLoggedIn()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (IsLogin())
            {
                try
                {
                    HttpClient httpClient = new HttpClient();

                    httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
                    string uri = String.Format("http://obscure-sea-2022.herokuapp.com/Notifications.json?username={0}&auth_token={1}", localSettings.Values["username"], localSettings.Values["token"]);
                    Debug.WriteLine(uri);
                    string ResponseString = await httpClient.GetStringAsync(
                                                new Uri(uri));

                    
                    Debug.WriteLine(ResponseString);
                    var notifications = JsonConvert.DeserializeObject<List<Notification>>(ResponseString);

                    NotificationListView.ItemsSource = notifications;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
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
            string userName = UserNameField.Text;
            var password = PassWordField.Password;

            PassWordField.Password = "";
            UserNameField.Text = "";


            var uri = new Uri("http://obscure-sea-2022.herokuapp.com/users/sign_in");
            var json = "{\"user\": {\"username\" :\"" + userName + "\", \"password\": \"" + password + "\" }}";

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
            var responseString = await httpClient.PostAsync(uri, new HttpStringContent(json, UnicodeEncoding.Utf8,
                                    "application/json"));
            string token = responseString.Content.ToString();
            Debug.WriteLine(token);
            keepData(token,userName);

            
            MessageBoxDisplay("ยินดีต้อนรับเข้าสู่ระบบ");

            LoginPanel.Visibility = Visibility.Collapsed;
            UserProFilePanel.Visibility = Visibility.Visible;
    
        }

        private async void ClickToLogOut(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("http://obscure-sea-2022.herokuapp.com/users/sign_out.json");
            var localSettings =  Windows.Storage.ApplicationData.Current.LocalSettings;

            HttpClient httpClient = new HttpClient();
 
            httpClient.DefaultRequestHeaders.Add("X-User-Username",localSettings.Values["username"].ToString());
            httpClient.DefaultRequestHeaders.Add("X-Token", localSettings.Values["token"].ToString());
            httpClient.DefaultRequestHeaders.Accept.TryParseAdd("application/json");

            var ResponseString = await httpClient.DeleteAsync(uri);

            localSettings.Values.Remove("token");
            localSettings.Values.Remove("username");

            MessageBoxDisplay("คุณได้ออกจากระบบแล้ว");

            LoginPanel.Visibility = Visibility.Visible;
            UserProFilePanel.Visibility = Visibility.Collapsed;

        }

        private bool IsLogin()
        {
            var localSettings =  Windows.Storage.ApplicationData.Current.LocalSettings;
            return localSettings.Values.ContainsKey("token") && localSettings.Values.ContainsKey("username");
          
        }

        private void keepData(string token,string username)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Create a simple setting
            if (!IsLogin())
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
