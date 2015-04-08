using Agro.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public sealed partial class DashboardPage : Page
    {

        public DashboardPage()
        {
            Initialize(null);
        }

        public DashboardPage(Dashboard dashboard)
        {
            Initialize(dashboard);
        }

        private void Initialize(Dashboard dashboard)
        {
            this.InitializeComponent();
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            
            if (dashboard != null)
            {
                Name.Text = dashboard.Name;
                PlantationName.Text = dashboard.PlantationName;
                PlantationDate.Text = dashboard.PlantationDate;
            }
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
    }
}
