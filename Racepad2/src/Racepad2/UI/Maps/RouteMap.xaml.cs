/*
* RouteMap - Copyright 2017 Supremacy Software
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

using System.Collections.Generic;
using System;

using Windows.UI;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

using Racepad2.Core.Navigation.Route;


namespace Racepad2.UI.Maps {

    /// <summary>
    /// Represents a map that can display a route and navigate a vehicle 
    /// throught that route. This map supports dynamic location
    /// setting and map snap-on and release. It can also display a route
    /// dynamically while drivint (for previewing the driven path).
    /// 
    /// It has 2 buttons on the top-right corner: 
    /// Preview route and Attach-Detach.
    /// 
    /// </summary>
    public sealed partial class RouteMap : UserControl {

        /// <summary>
        /// The vehicle position on the map, this describes
        /// geoposition and heading.
        /// </summary>
        public VehiclePosition Position {
            get {
                return _position;
            }
            set {
                UpdatePosition(value);
                _position = value;
            }
        }

        /// <summary>
        /// Displays the route on the map without snapping to it.
        /// </summary>
        public DriveRoute Route {
            get {
                return _route;
            }
            set {
                _route = value;
                SetRoute(value);
            }
        }

        /// <summary>
        /// Sets the path to be previewed
        /// </summary>
        public List<BasicGeoposition> PreviewPath {
            get {
                return _previewPath;
            }
            set {
                if (value.Count > 10)
                    TogglePathPreviewButton(true);
                _previewPath = value;
            }
        }

        public RouteMap() {
            this.InitializeComponent();
            this.InitializeUserComponents();
        }

        /// <summary>
        /// Initializes the misc components
        /// </summary>
        private void InitializeUserComponents() {
            UserLocation = new MapIcon() {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/arrow.png"))
            };
            Map.MapElements.Add(UserLocation);
            Mode = MapMode.MAP_ATTACHED;
        }

        private DriveRoute _route;
        private VehiclePosition _position;
        private List<BasicGeoposition> _previewPath;
        private MapIcon UserLocation { get; set; }
        private DriveRoute RouteBackup { get; set; }
        private bool PathPreview { get; set; } = false;
        private MapMode _mode;

        /// <summary>
        /// This sets the map mode, attached or detached.
        /// </summary>
        private MapMode Mode {
            get {
                return _mode;
            }
            set {
                switch (value) {
                    case MapMode.MAP_ATTACHED:
                        Map.ZoomInteractionMode = MapInteractionMode.Disabled;
                        Map.PanInteractionMode = MapPanInteractionMode.Disabled;
                        Map.TiltInteractionMode = MapInteractionMode.Disabled;
                        Map.RotateInteractionMode = MapInteractionMode.Disabled;
                        DetachButton.Background = new SolidColorBrush(Colors.Red);
                        UpdatePosition(Position);
                        break;
                    case MapMode.MAP_DETACHED:
                        Map.ZoomInteractionMode = MapInteractionMode.GestureOnly;
                        Map.PanInteractionMode = MapPanInteractionMode.Auto;
                        Map.TiltInteractionMode = MapInteractionMode.GestureOnly;
                        Map.RotateInteractionMode = MapInteractionMode.GestureOnly;
                        DetachButton.Background = new SolidColorBrush(Colors.Green);
                        Map.Heading = 0;
                        break;
                }
                _mode = value;
            }
        }

        /// <summary>
        /// This describes the two modes in which the map can be
        /// </summary>
        private enum MapMode {
            MAP_ATTACHED, MAP_DETACHED
        }

        /// <summary>
        /// This is triggered when the PathPreview button is pressed.
        /// </summary>
        private async void PathPreviewButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            if (PreviewPath == null) return;
            if (!PathPreview) {
                PathPreview = true;
                Mode = MapMode.MAP_DETACHED;
                Map.Heading = 0;
                RouteBackup = Route;
                RemoveAllMapElements();
                Geopath path = new Geopath(PreviewPath);
                MapPolyline line = new MapPolyline() {
                    StrokeColor = Colors.Purple,
                    StrokeThickness = 5,
                    Path = path
                };
                GeoboundingBox box = GeoboundingBox.TryCompute(PreviewPath);
                Map.MapElements.Add(line);
                await Map.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(20), MapAnimationKind.None);
                DetachButton.IsEnabled = false;
            } else {
                PathPreview = false;
                DetachButton.IsEnabled = true;
                Mode = MapMode.MAP_ATTACHED;
                RemoveAllMapElements();
                Map.MapElements.Add(UserLocation);
                if (RouteBackup != null) {
                    Route = RouteBackup;
                }
            }
        }

        /// <summary>
        /// Event occurs when the attach/detach button is pressed
        /// </summary>
        private void DetachButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            if (Mode == MapMode.MAP_DETACHED) {
                Mode = MapMode.MAP_ATTACHED;
            } else {
                Mode = MapMode.MAP_DETACHED;
            }
        }

        /// <summary>
        /// If attached, centers the map onto the users location and 
        /// turns the map in the heading direction
        /// </summary>
        private async void UpdatePosition(VehiclePosition position) {
            if (position == null) return;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                if (Mode == MapMode.MAP_ATTACHED) {
                    if (position.Bearing != null) {
                        await Map.TrySetViewAsync(position.Position, 15, (double)position.Bearing, 0, MapAnimationKind.None);
                    }
                }
                UserLocation.Location = position.Position;
            });
        }

        /// <summary>
        /// Places the colored polyline route on the map
        /// </summary>
        /// <param name="value"></param>
        private void SetRoute(DriveRoute value) {
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
        }

        /// <summary>
        /// This toggles the preview button on the map
        /// </summary>
        /// <param name="state"></param>
        public async void TogglePathPreviewButton(bool state) {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                PathPreviewButton.IsEnabled = state;
            });
        }

        /// <summary>
        /// This removes all dynamically placed items from the map.
        /// </summary>
        private void RemoveAllMapElements() {
            Map.MapElements.Clear();
        }

    }
}
