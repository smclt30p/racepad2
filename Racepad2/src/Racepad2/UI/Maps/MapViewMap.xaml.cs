/*
* MapViewMap - Copyright 2017 Supremacy Software
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
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;


namespace Racepad2.UI.Maps {

    /// <summary>
    /// This is a map used for general map display and waypoint 
    /// adding.
    /// </summary>
    public sealed partial class MapViewMap : UserControl {

        /// <summary>
        /// Draw a route on the map. This property is write-only.
        /// </summary>
        public Geopath Route {
            get {
                return _path;
            }
            set {
                if (value == null) {
                    Map.MapElements.Remove(_currentPoly);
                    _path = value;
                    return;
                }
                Map.MapElements.Remove(_currentPoly);
                _currentPoly = new MapPolyline() {
                    StrokeColor = Colors.Red,
                    Path = new Geopath(value.Positions),
                    StrokeThickness = 5
                };
                Map.MapElements.Add(_currentPoly);
                _path = value;
            }
        }

        /// <summary>
        /// The displayed map elements
        /// </summary>
        public IList<MapElement> MapElements {
            get {
                return Map.MapElements;
            }
        }

        /// <summary>
        /// The camera for backup
        /// </summary>
        public Camera Camera {
            get {
                Camera cam = new Camera();
                cam.Location = Map.ActualCamera.Location.Position;
                cam.Heading = Map.ActualCamera.Heading;
                return cam;
            } set {
                SetMapCamera(value);
            }
        }

        /// <summary>
        /// Define the FromPointSelected event
        /// </summary>
        public delegate void FromPointSelectedDelegate(object sender, PointSelectedEventArgs args);
        public event FromPointSelectedDelegate FromPointSelected;
        private void OnFromPointSelected(Geopoint point) {
            if (FromPointSelected == null) return;
            PointSelectedEventArgs args = new PointSelectedEventArgs();
            args.Location = point;
            FromPointSelected(this, args);
        }

        /// <summary>
        /// Define the ToPointSelected event
        /// </summary>
        public delegate void ToPointSelectedDelegate(object sender, PointSelectedEventArgs args);
        public event ToPointSelectedDelegate ToPointSelected;
        private void OnToPointSelected(Geopoint point) {
            if (ToPointSelected == null) return;
            PointSelectedEventArgs args = new PointSelectedEventArgs() {
                Location = point
            };
            ToPointSelected(this, args);
        }

        /// <summary>
        /// Define the ToPointSelected event
        /// </summary>
        public delegate void PivotPointSelectedDelegate(object sender, PointSelectedEventArgs args);
        public event PivotPointSelectedDelegate PivotPointSelected;
        private void OnPivotPointSelected(Geopoint point) {
            if (PivotPointSelected == null) return;
            PointSelectedEventArgs args = new PointSelectedEventArgs() {
                Location = point
            };
            PivotPointSelected(this, args);
        }

        public delegate void BookmarkAddedDelegate(object sender, PointSelectedEventArgs args);
        public event BookmarkAddedDelegate BookmarkAdded;
        private void OnBookmarkAdded(Geopoint point) {
            if (BookmarkAdded == null) return;
            PointSelectedEventArgs args = new PointSelectedEventArgs() {
                Location = point
            };
            BookmarkAdded(this, args);
        }

        public MapViewMap() {
            this.InitializeComponent();
            Map.MapRightTapped += _innerMap_MapRightTapped;
        }

        private Geopath _path;
        private MapPolyline _currentPoly;

        /// <summary>
        /// Occurs when the users right clicks on the map
        /// on some point
        /// </summary>
        private void _innerMap_MapRightTapped(MapControl sender, MapRightTappedEventArgs args) {
            ShowFlyoutMenu(args.Position, args.Location);
        }

        /// <summary>
        /// Display a popup menu with 3 items, from to and pivot
        /// </summary>
        private void ShowFlyoutMenu(Point position, Geopoint location) {
            MenuFlyout flyout = new MenuFlyout();
            MenuFlyoutItem from = new MenuFlyoutItem();
            MenuFlyoutItem to = new MenuFlyoutItem();
            MenuFlyoutItem pivot = new MenuFlyoutItem();
            MenuFlyoutItem bookmark = new MenuFlyoutItem();
            from.Text = "Directions from";
            to.Text = "Directions to";
            pivot.Text = "Add pivot point";
            bookmark.Text = "Add bookmark";
            pivot.Click += Pivot_Tapped;
            from.Click += From_Tapped;
            to.Click += To_Tapped;
            bookmark.Click += Bookmark_Click;
            flyout.Items.Add(from);
            flyout.Items.Add(to);
            flyout.Items.Add(pivot);
            flyout.Items.Add(bookmark);
            flyout.ShowAt(Map, position);
            void Pivot_Tapped(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            {
                OnPivotPointSelected(location);
            }
            void From_Tapped(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            {
                OnFromPointSelected(location);
            }
            void To_Tapped(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            {
                OnToPointSelected(location);
            }
            void Bookmark_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
            {
                OnBookmarkAdded(location);
            }
        }

        internal async void SetViewBox(GeoboundingBox box) {
            await Map.TrySetViewBoundsAsync(box, new Thickness(20), MapAnimationKind.Default);
        }

        private async void SetMapCamera(Camera camera) {
            await Map.TrySetViewAsync(new Geopoint(camera.Location), 13, null, null, MapAnimationKind.None);
        }
    }

    /// <summary>
    /// Defines a map point selected event argument
    /// </summary>
    public class PointSelectedEventArgs : EventArgs {
        public Geopoint Location { get; set; }
    }

    /// <summary>
    /// Defines a map camera
    /// </summary>
    public class Camera {
        public BasicGeoposition Location { get; set; }
        public double Heading { get; set; }
    }

}
