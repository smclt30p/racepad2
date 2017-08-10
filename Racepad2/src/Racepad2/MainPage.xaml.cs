using Windows.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Racepad2 {
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
        }



        private void MainPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e) {
            Frame frame = (Frame)Window.Current.Content;
            if (frame.CanGoBack && e.Handled == false) {
                frame.GoBack();
                e.Handled = true;
            }
        }

        private void Ride_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(NavigationPage), null);
        }

        private void History_Click(object sender, RoutedEventArgs e) {
        }

        private void Settings_Click(object sender, RoutedEventArgs e) {
        }

        private void Courses_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(RouteBrowser));
        }
    }
}
