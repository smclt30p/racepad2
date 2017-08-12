/*
* RoutePreviewMap - Copyright 2017 Supremacy Software
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

using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

using Racepad2.Core.Navigation.Route;


namespace Racepad2.UI.Maps {

    /// <summary>
    /// This is a map that can display a route on it, without
    /// the ability to provide navigation. It is used for 
    /// previewing of routes in the route browser.
    /// </summary>
    public sealed partial class RoutePreviewMap : UserControl {

        /// <summary>
        /// Sets the route that is beign displayed
        /// </summary>
        public DriveRoute Route {
            get {
                return _route;
            }
            set {
                SetRoute(value);
                _route = value;
            }
        }

        public RoutePreviewMap() {
            this.InitializeComponent();
        }

        private DriveRoute _route;

        /// <summary>
        /// Displays the colored polyline route on the map
        /// and tries to center the map to be previewing the
        /// polyline.
        /// </summary>
        /// <param name="value"></param>
        private async void SetRoute(DriveRoute value) {
            List<GeopositionVector> gradiends = DriveRoute.GetVectorsFromRoute(value.Path);
            MapPolyline polyline;
            Geopath path;
            foreach (GeopositionVector pair in gradiends) {
                path = new Geopath(pair.Path);
                polyline = new MapPolyline() {
                    StrokeThickness = 5,
                    Path = path,
                    StrokeColor = DriveRoute.GetColorFromSlope(pair.SlopePercentage)
                };
                Map.MapElements.Add(polyline);
            }
            GeoboundingBox box = GeoboundingBox.TryCompute(value.Path);
            await Map.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(30), MapAnimationKind.None);
        }
    }

}
