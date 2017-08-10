using Racepad2.Core;
using Racepad2.Geo.Navigation;
using Racepad2.Geo.Navigation.Core;
using Racepad2.Route;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Racepad2 {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RoutePreview : Page {

        public RoutePreview() {
            this.InitializeComponent();
        }

        private DriveRoute Route { get; set; }
        private List<GradientPair> Gradiends { get; set; }

        protected async override void OnNavigatedTo(NavigationEventArgs e) {

            Go.IsEnabled = false;

            IStorageItem item = e.Parameter as IStorageItem;
            string xml = await FileReader.ReadFile(item);

            GPXRouteParser parser = new GPXRouteParser(xml);

            try {

                Route = await parser.ParseAsync();
                Map.Route = Route;
                Gradiends = DriveRoute.GetGradientPairsFromRoute(Route.Path);

                double distance = Math.Round(DriveRoute.GetLength(Route) / 1000, 2);
                double avgslope = 0;

                foreach (GradientPair pair in Gradiends) {
                    avgslope += pair.SlopePercentage ;
                }

                avgslope = Math.Round(avgslope / Gradiends.Count, 0);

                Desc.Text = item.Name.Replace(".gpx", "");
                Info.Text = String.Format("{0}km - Avg. slope: {1}%", distance, avgslope);

                Progress.Visibility = Visibility.Collapsed;
                Go.IsEnabled = true;

            } catch (GPXException) {
                Progress.Visibility = Visibility.Collapsed;
                Desc.Text = "GPX Error"; 
                Go.IsEnabled = false;
            }

        }

        private void Go_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(NavigationPage), Route);
        }
    }
}
