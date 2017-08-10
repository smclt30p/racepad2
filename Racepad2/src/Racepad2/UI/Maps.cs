/*
* Maps - Copyright 2017 Supremacy Software
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
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

using Racepad2.Core.Navigation.Route;


namespace Racepad2.UI.Maps {

    /// <summary>
    /// The MapUtil class is a common class for all maps
    /// that need some special tools and a map service token
    /// </summary>
    class MapUtil {

        /// <summary>
        /// The map service token we got from Microsoft
        /// </summary>
        public static string MAP_SERVICE_TOKEN =
            "4qiurkdgKlTD5Ba1qkOu~7va9tRX0jj2kZEnXqyt8Iw~AmfqIUMnMk6zor_" +
            "i4yKPKpXJeT3J0FxOsy8Z6BcGKRJ6yZwKRfZsj-ak87UiVnzT";

        /// <summary>
        /// Get a color from a percentage slope. This is used to
        /// visualize slopes on a map.
        /// </summary>
        public static Color GetColorFromSlope(double percentage) {
            if (percentage < -20) return GetSolidColorBrush("0000FF");
            if (percentage > 20) return GetSolidColorBrush("FF0000");
            if (percentage >= -20 && percentage <= -15) return GetSolidColorBrush("00fffF");
            if (percentage >= -15 && percentage <= -10) return GetSolidColorBrush("0060ff");
            if (percentage >= -10 && percentage <= -5) return GetSolidColorBrush("00ff12");
            if (percentage >= -5 && percentage <= 0) return GetSolidColorBrush("00ff12");
            if (percentage >= 0 && percentage <= 5) return GetSolidColorBrush("fffc00");
            if (percentage >= 5 && percentage <= 10) return GetSolidColorBrush("fffc00");
            if (percentage >= 10 && percentage <= 15) return GetSolidColorBrush("ff9600");
            if (percentage >= 15 && percentage <= 20) return GetSolidColorBrush("ff00f0");
            return Colors.Purple;
        }
        
        /// <summary>
        /// Returns a color from a hex color description format, for ex: #FF00FF
        /// </summary>
        public static Color GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = 255;
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush.Color;
        }
    }

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
    class RouteMap : Grid {

        public RouteMap() : base() {
            InitializeComponent();
        }

        /// <summary>
        /// The vehicle position on the map, this describes
        /// geoposition and heading.
        /// TODO: Move the path details off of the UI into the Ride class
        /// </summary>
        public VehiclePosition Position {
            get {
                return _position;
            }
            set {
                if (_userPathData == null) _userPathData = new List<BasicGeoposition>();
                _userPathData.Add(value.Position.Position);
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

        public CoreDispatcher ViewDispatcher { get; set; }
        private DriveRoute _route;
        private VehiclePosition _position;
        private List<BasicGeoposition> _userPathData;
        private string AttachIcon = null;
        private string DetachIcon = null;
        private string PathPreviewIcon = null;
        private MapMode _mode;
        private MapControl Map { get; set; }
        private StackPanel ButtonContainer { get; set; }
        private Button DetachButton { get; set; }
        private Button PathPreviewButton { get; set; }
        private MapIcon UserLocation { get; set; }
        private DriveRoute RouteBackup { get; set; }
        private bool PathPreview { get; set; } = false;

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
                        DetachButton.Content = DetachIcon;
                        break;
                    case MapMode.MAP_DETACHED:
                        Map.ZoomInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                        Map.PanInteractionMode = MapPanInteractionMode.Auto;
                        Map.TiltInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                        Map.RotateInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                        DetachButton.Content = AttachIcon;
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
        /// This removes all dynamically placed items from the map.
        /// </summary>
        private void RemoveAllMapElements() {
            Map.MapElements.Clear();
        }
        
        /// <summary>
        /// Initialize the view hierarchy and the component
        /// children elements.
        /// </summary>
        private void InitializeComponent() {
            /* Initialize icons */
            DetachIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE17C", 16)).ToString();
            AttachIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE18D", 16)).ToString();
            PathPreviewIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE81B", 16)).ToString();
            /* Initialize the grid */
            RowDefinition row1 = new RowDefinition();
            base.RowDefinitions.Add(row1);
            /* Initialize the map */
            Map = new MapControl() {
                MapServiceToken = MapUtil.MAP_SERVICE_TOKEN,
                ZoomInteractionMode = MapInteractionMode.Disabled,
                PanInteractionMode = MapPanInteractionMode.Disabled,
                TiltInteractionMode = MapInteractionMode.Disabled,
                RotateInteractionMode = MapInteractionMode.Disabled,
                ColorScheme = MapColorScheme.Dark
            };
            /* Initialize the buttons */
            ButtonContainer = new StackPanel() {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                Margin = new Windows.UI.Xaml.Thickness(10),
                Orientation = Orientation.Horizontal
            };
            DetachButton = new Button() {
                FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 20,
                Content = DetachIcon,
                Height = 40,
                Width = 40,
                Background = new SolidColorBrush(Colors.DarkGray)
            };
            PathPreviewButton = new Button() {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 20,
                Content = PathPreviewIcon,
                Height = 40,
                Width = 40,
                Margin = new Windows.UI.Xaml.Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Colors.DarkGray)
            };
            UserLocation = new MapIcon() {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/arrow.png"))
            };
            Map.MapElements.Add(UserLocation);
            ButtonContainer.Children.Insert(0, PathPreviewButton);
            ButtonContainer.Children.Insert(1, DetachButton);
            DetachButton.Click += DetachButton_Click;
            PathPreviewButton.Click += PathPreviewButton_Click;
            /* Create view tree */
            base.Children.Add(Map);
            base.Children.Add(ButtonContainer);
            Mode = MapMode.MAP_ATTACHED;
        }

        /// <summary>
        /// This is triggered when the PathPreview button is pressed.
        /// </summary>
        private async void PathPreviewButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            if (!PathPreview) {
                if (_userPathData.Count < 2) {
                    return;
                }
                PathPreview = true;
                Mode = MapMode.MAP_DETACHED;
                Map.Heading = 0;
                RouteBackup = Route;
                RemoveAllMapElements();
                Geopath path = new Geopath(_userPathData);
                MapPolyline line = new MapPolyline() {
                    StrokeColor = Colors.Purple,
                    StrokeThickness = 5,
                    Path = path
                };
                GeoboundingBox box = GeoboundingBox.TryCompute(_userPathData);
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
            await ViewDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
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
            List<GradientPair> gradiends = DriveRoute.GetGradientPairsFromRoute(value.Path);
            MapPolyline polyline;
            Geopath path;
            foreach (GradientPair pair in gradiends) {
                path = new Geopath(pair.Path);
                polyline = new MapPolyline() {
                    StrokeThickness = 5,
                    Path = path,
                    StrokeColor = MapUtil.GetColorFromSlope(pair.SlopePercentage)
                };
                Map.MapElements.Add(polyline);
            }
        }
    }

    /// <summary>
    /// This is a map that can display a route on it, without
    /// the ability to provide navigation. It is used for 
    /// previewing of routes in the route browser.
    /// </summary>
    class RoutePreviewMap : Grid {

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
            InitializeComponent();
        }

        private MapControl _innerMap;
        private DriveRoute _route;

        /// <summary>
        /// Initializes the inner map control
        /// system
        /// </summary>
        private void InitializeComponent() {
            _innerMap = new MapControl() {
                MapServiceToken = MapUtil.MAP_SERVICE_TOKEN,
                ColorScheme = MapColorScheme.Dark
            };
            base.Children.Add(_innerMap);
        }

        /// <summary>
        /// Displays the colored polyline route on the map
        /// and tries to center the map to be previewing the
        /// polyline.
        /// </summary>
        /// <param name="value"></param>
        private async void SetRoute(DriveRoute value) {
            List<GradientPair> gradiends = DriveRoute.GetGradientPairsFromRoute(value.Path);
            MapPolyline polyline;
            Geopath path;
            foreach (GradientPair pair in gradiends) {
                path = new Geopath(pair.Path);
                polyline = new MapPolyline() {
                    StrokeThickness = 5,
                    Path = path,
                    StrokeColor = MapUtil.GetColorFromSlope(pair.SlopePercentage)
                };
                _innerMap.MapElements.Add(polyline);
            }
            GeoboundingBox box = GeoboundingBox.TryCompute(value.Path);
            await _innerMap.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(30), MapAnimationKind.None);
        }
    }

    /// <summary>
    /// This class describes a vehicle position on the map.
    /// </summary>
    public class VehiclePosition {

        public VehiclePosition() {
        }

        public VehiclePosition(Geopoint position, double? bearing) {
            Position = position;
            Bearing = bearing;
        }

        public Geopoint Position { get; set; }
        public double? Bearing { get; set; }

    }
}