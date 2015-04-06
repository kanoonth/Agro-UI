using Agro.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Agro
{

    public sealed partial class FeedPage : Page
    {
        FeedItem FeedItem;

        public FeedPage()
        {
            this.InitializeComponent();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FeedItem = e.Parameter as FeedItem;
            FeedNewsTitle.Text = FeedItem.Title;
            FeedInformation.Text = FeedItem.Body;

            if (FeedItem.ImageURL != null)
            {
                BitmapImage img = new BitmapImage();
                img.UriSource = new Uri(FeedItem.ImageURL);
                FeedPicture.Source = img;
            }
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.GoBack();
            }
        }
    }
}
