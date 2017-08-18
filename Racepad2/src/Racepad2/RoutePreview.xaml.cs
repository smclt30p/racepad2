/*
* RoutePreview - Copyright 2017 Supremacy Software
*     ______  _____  ___  ______  ______  _______  __
*    / __/ / / / _ \/ _ \/ __/  |/  / _ |/ ___/\ \/ /
*   _\ \/ /_/ / ___/ , _/ _// /|_/ / __ / /__   \  /
*  /___/\____/_/  /_/|_/___/_/  /_/_/ |_\___/   /_/.org
*
*                 Software Supremacy
*                 www.supremacy.org
* 
* This file is part of Racepad2
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Racepad2.Core.Navigation.Parsers;
using Racepad2.Core.Navigation.Route;


namespace Racepad2 {

    /// <summary>
    /// This class represents a route preview that displays
    /// very basic info about a route, the route itself and a 
    /// red "GO" button.
    /// </summary>
    public sealed partial class RoutePreview : Page {

        public RoutePreview() {
            this.InitializeComponent();
        }

        private DriveRoute Route { get; set; }

        /// <summary>
        /// This is fired when the page has loaded. This
        /// parses the GPX file and displays the route on the map.
        /// </summary>
        protected async override void OnNavigatedTo(NavigationEventArgs e) {
            Go.IsEnabled = false;
            IStorageItem item = e.Parameter as IStorageItem;
            /* Step 1: Read the GPX file */
            Route = await RouteParser.ParseRouteFromFileAsync(item, RouteFileType.GPX);
            if (Route == null) {
                Progress.Visibility = Visibility.Collapsed;
                Desc.Text = "GPX Error";
                Go.IsEnabled = false;
                return;
            }
            Map.Route = Route;
            /* Step 2: Populate the UI */
            Desc.Text = item.Name.Replace(".gpx", "");
            Info.Text = String.Format("{0}km - Avg. slope: {1}%", 
                Math.Round(DriveRoute.GetLength(Route) / 1000, 2), 
                DriveRoute.GetAverageSlope(new Windows.Devices.Geolocation.Geopath(Route.Path)));
            Progress.Visibility = Visibility.Collapsed;
            Go.IsEnabled = true;
        }

        /// <summary>
        /// Occurs when the GO button is pressed.
        /// </summary>
        private async void Go_Click(object sender, RoutedEventArgs e) {
            NavigationPageParameter param = new NavigationPageParameter {
                Route = Route,
                Type = NavigationPageParameterType.NewSession
            };
            param.NavigationDisabled = await param.PromptNavigation();
            Frame.Navigate(typeof(NavigationPage), param);
        }
    }
}
