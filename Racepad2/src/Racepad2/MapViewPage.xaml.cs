/*
* MapViewPage - Copyright 2017 Supremacy Software
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
using System.Collections.Generic;

using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Streams;


namespace Racepad2 {

    /// <summary>
    /// A page that contains a few navigation elements and the general map
    /// </summary>
    public sealed partial class MapViewPage : Page {

        private Geopoint start;
        private Geopoint end;
        private MapIcon startIcon;
        private MapIcon endIcon;
        private List<Geopoint> waypoints;

        public MapViewPage() {
            this.InitializeComponent();
            waypoints = new List<Geopoint>();
        }

        private async void TestOut() {
            MapRouteFinderResult result = await MapRouteFinder.GetDrivingRouteFromWaypointsAsync(waypoints);
        }

        private void Map_PivotPointSelected(object sender, UI.Maps.PointSelectedEventArgs args) {
            waypoints.Add(args.Location);
        }

        private void Map_FromPointSelected(object sender, UI.Maps.PointSelectedEventArgs args) {
            startIcon = new MapIcon() {
                Location = args.Location,
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/A.png")),
                Title = "1"
            };
            waypoints.Add(args.Location);
        }

        private void Map_ToPointSelected(object sender, UI.Maps.PointSelectedEventArgs args) {
            endIcon = new MapIcon() {
                Location = args.Location,
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/B.png"))
            };
            waypoints.Add(args.Location);
        }

        private void GetDirection_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            TestOut();
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Pane.IsPaneOpen = !Pane.IsPaneOpen;
        }
    }
}
