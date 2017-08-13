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
using System.Collections.Specialized;
using System.Collections.ObjectModel;

using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Storage.Streams;

using Racepad2.UI.Controls;
using Racepad2.Core.Navigation.Route;
using Racepad2.Core.Util;
using System.Xml.Serialization;

namespace Racepad2 {

    /// <summary>
    /// A page that contains a few navigation elements and the general map
    /// </summary>
    public sealed partial class MapViewPage : Page {

        /// <summary>
        /// The main deciding factor of the whole planner, the
        /// waypoints that have been placed on the map.
        /// </summary>
        private ObservableCollection<Waypoint> Waypoints { get; set; }

        /// <summary>
        /// The list that holds all the bookmarks on the map
        /// </summary>
        private List<Bookmark> Bookmarks { get; set; }

        /// <summary>
        /// The saved route after calculation
        /// </summary>
        private Geopath Route {
            get { return new Geopath(_route.Path); }
            set {
                _route = new DriveRoute() {
                    Path = value.Positions,
                };
                _route.Corners = DriveRoute.ParseCorners(_route.Path);
                _route.Status = CourseStatus.COURSE_NOT_STARTED;
                double lengthKilometers = Math.Round(DriveRoute.GetLength(_route) / 1000, 2);
                LenText.Text = String.Format("Distance: {0}km", lengthKilometers);
                DuraText.Text = CalculateNeededTime(lengthKilometers);
            }
        }

        /// <summary>
        /// Convert length in kilometers to visual time needed
        /// to complete the ride on a MTB
        /// </summary>
        private string CalculateNeededTime(double lengthKilometers) {
            double neededMinutes = lengthKilometers / 23.0 * 60; // length divided by speed times minutes
            double hours = Math.Round(neededMinutes / 60);
            double minutes = Math.Round(neededMinutes % 60);
            return String.Format("Estimated time: {0} h {1} min", hours, minutes);
        }

        public MapViewPage() {
            this.InitializeComponent();
            LoadBookmarks();
            Waypoints = new ObservableCollection<Waypoint>();
            Waypoints.CollectionChanged += Waypoints_CollectionChanged;
            Map.ToPointSelected += Map_ToPointSelected;
            Map.FromPointSelected += Map_FromPointSelected;
            Map.PivotPointSelected += Map_PivotPointSelected;
            InvalidateView();
        }

        private DriveRoute _route;

        /// <summary>
        /// This event is triggered each time the collection changes.
        /// We calculate the new route if there is more than 2 points, else
        /// we clear the route from the map.
        /// </summary>
        private async void Waypoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (Waypoints.Count > 1) {
                List<Geopoint> path = new List<Geopoint>();
                foreach (Waypoint p in Waypoints) {
                    path.Add(p.Position);
                }
                Progress.Visibility = Visibility.Visible;
                Title.Text = "Routing, please wait...";
                MapRouteFinderResult result = await MapRouteFinder.GetDrivingRouteFromWaypointsAsync(path);
                Progress.Visibility = Visibility.Collapsed;
                switch (result.Status) {
                    case MapRouteFinderStatus.Success:
                        Title.Text = "To start, press the red start button";
                        Map.Route = result.Route.Path;
                        Route = result.Route.Path;
                        ValidateView();
                        break;
                    default:
                        Title.Text = "Routing failed";
                        InvalidateView();
                        break;
                }
            } else {
                Map.Route = null;
                InvalidateView();
            }
        }

        /// <summary>
        /// This gets triggered when a map requests to add a pivot point to the map.
        /// Also called a "Via" point. Represented as a 'V' on the map.
        /// </summary>
        private void Map_PivotPointSelected(object sender, UI.Maps.PointSelectedEventArgs args) {
            // There is no vias, add a start instead of a pivot
            if (Waypoints.Count == 0) {
                AddWaypoint(0, new Waypoint { Type = ViaType.Start, Position = args.Location });
                return;
            }
            // There is one pivot, either a start or an end
            if (Waypoints.Count == 1) {
                switch (Waypoints[0].Type) {
                    // if its a start pivot, add an end
                    case ViaType.Start:
                        AddWaypoint(1, new Waypoint { Type = ViaType.End, Position = args.Location });
                        break;
                     // if its a end pivot, add a start
                    case ViaType.End:
                        AddWaypoint(1, new Waypoint { Type = ViaType.Start, Position = args.Location });
                        break;
                }
                return;
            }

            AddWaypoint(Waypoints.Count - 1, new Waypoint { Type = ViaType.Pivot, Position = args.Location });
        }

        /// <summary>
        /// This gets triggered when a map requests to add a start point to the map.
        /// Represented as a 'A' on the map.
        /// </summary>
        private void Map_FromPointSelected(object sender, UI.Maps.PointSelectedEventArgs args) {
            // there is no pivots, add a start one
            if (Waypoints.Count == 0) {
                AddWaypoint(0, new Waypoint { Type = ViaType.Start, Position = args.Location });
                return;
            }
            // the first point is NOT a start pivot, dont overwrite it
            else if (Waypoints.Count >= 1 && Waypoints[0].Type != ViaType.Start) {
                AddWaypoint(0, new Waypoint { Type = ViaType.Start, Position = args.Location });
                return;
            }
            // the first point IS a start pivot, overwrite it
            else {
                RemoveWaypoint(0);
                AddWaypoint(0, new Waypoint { Type = ViaType.Start, Position = args.Location });
                return;
            }
        }

        /// <summary>
        /// This gets triggered when a map requests to add a stop point to the map.
        /// Represented as a 'B' on the map.
        /// </summary>
        private void Map_ToPointSelected(object sender, UI.Maps.PointSelectedEventArgs args) {
            // there is nothing
            if (Waypoints.Count == 0) {
                AddWaypoint(0, new Waypoint { Type = ViaType.End, Position = args.Location });
                return;
            }
            // there is one but its not an end pivot
            else if (Waypoints.Count >= 1 && Waypoints[Waypoints.Count - 1].Type != ViaType.End) {
                AddWaypoint(Waypoints.Count, ViaType.End, args.Location);
                return;
            } 
            // there is one and its an aned pivot, overwrite it
            else {
                RemoveWaypoint(Waypoints.Count - 1);
                AddWaypoint(Waypoints.Count, ViaType.End, args.Location);
                return;
            }
        }

        /// <summary>
        /// Removes the given waypoint from the map.
        /// </summary>
        private void RemoveWaypoint(Waypoint point) {
            Waypoints.Remove(point);
            Map.MapElements.Remove(point.Icon);
        }

        /// <summary>
        /// Inserts the given waypoint into the waypoints list
        /// and adds a icon to the map according to the waypoint type.
        /// </summary>
        private void AddWaypoint(int index, Waypoint point) {
            AddWaypoint(index, point.Type, point.Position);
        }

        /// <summary>
        /// Inserts the given waypoint into the waypoints list
        /// and adds a icon to the map according to the waypoint type.
        /// </summary>
        private void AddWaypoint(int index, ViaType type, Geopoint location) {
            MapIcon icon = new MapIcon() {
                Location = location
            };
            switch (type) {
                case ViaType.Start:
                    icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/A.png"));
                    break;
                case ViaType.End:
                    icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/B.png"));
                    break;
                case ViaType.Pivot:
                    icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/V.png"));
                    break;
            }
            Waypoints.Insert(index, new Waypoint { Type = type, Position = location, Icon = icon });
            Map.MapElements.Add(icon);
        }

        /// <summary>
        /// Removes the given waypoint from the map.
        /// </summary>
        private void RemoveWaypoint(int index) {
            Waypoint wp = Waypoints[index];
            RemoveWaypoint(wp);
        }

        /// <summary>
        /// This is the pane open-close button event.
        /// </summary>
        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Pane.IsPaneOpen = !Pane.IsPaneOpen;
        }

        /// <summary>
        /// This event is triggered when the small "X" next to a via point on the 
        /// left-hand side menu is pressed.
        /// </summary>
        private void MenuViaItem_ItemRemoveRequested(object sender, ItemRemovedArgs args) {
            int index = Waypoints.IndexOf(args.Waypoint);
            // The waypoint is the start waypoint and has been removed and a pivot exists
            if (index == 0 && Waypoints.Count > 2) {
                // remove the start waypoint
                RemoveWaypoint(args.Waypoint);
                // transform pivot to start
                Waypoint newStart = Waypoints[0];
                RemoveWaypoint(newStart);
                newStart.Type = ViaType.Start;
                AddWaypoint(0, newStart);
            }
            // The waypoint is the end waypoint and has been removed and a pivot exists
            else if (index == Waypoints.Count - 1 && Waypoints.Count > 2) {

                // remove the end waypoint
                RemoveWaypoint(args.Waypoint);
                // transform last pivot to end
                Waypoint newEnd = Waypoints[Waypoints.Count - 1];
                RemoveWaypoint(newEnd);
                newEnd.Type = ViaType.End;
                AddWaypoint(Waypoints.Count, newEnd);
            } else {
                RemoveWaypoint(args.Waypoint);
            }

        }

        /// <summary>
        /// This occurs when the GO button is pressed
        /// </summary>
        private void GoButton_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(NavigationPage), _route);
        }

        /// <summary>
        /// This occurs when the trash button is pressed, deletes all vias.
        /// </summary>
        private void DeleteAllButton_Click(object sender, RoutedEventArgs e) {
            RemoveAllWaypoints();
        }

        /// <summary>
        /// This occurs when the reverse button is pressed, reverses the via order
        /// </summary>
        private void ReverseWaypoints_Click(object sender, RoutedEventArgs e) {
            List<Waypoint> oldWaypoints = new List<Waypoint>(Waypoints);
            RemoveAllWaypoints();
            for (int i = oldWaypoints.Count - 1, j = 0 ; i >= 0; i--, j++) {
                // set the icons
                if (j == 0) {
                    oldWaypoints[i].Type = ViaType.Start;
                } else if (j == oldWaypoints.Count - 1) {
                    oldWaypoints[i].Type = ViaType.End;
                }
                AddWaypoint(j, oldWaypoints[i]);
            }
        }

        /// <summary>
        /// Changes the type of the given waypoint
        /// </summary>
        private void ChangeWaypointType(Waypoint point, ViaType newtype) {
            int idx = Waypoints.IndexOf(point);
            Waypoint old = Waypoints[idx];
            RemoveWaypoint(point);
            old.Type = newtype;
            AddWaypoint(idx, old);
        }

        /// <summary>
        /// Removes everything from a map
        /// </summary>
        private void RemoveAllWaypoints() {
            foreach (Waypoint point in Waypoints) {
                Map.MapElements.Remove(point.Icon);
            }
            Waypoints.Clear();
        }

        /// <summary>
        /// This centers the active path in view
        /// </summary>
        private void CenterMap_Click(object sender, RoutedEventArgs e) {
            GeoboundingBox box = GeoboundingBox.TryCompute(_route.Path);
            Map.SetViewBox(box);
        }

        /// <summary>
        /// Invalides the UI
        /// </summary>
        private void InvalidateView() {
            DeleteAllButton.IsEnabled = false;
            GoButton.IsEnabled = false;
            ReverseWaypoints.IsEnabled = false;
            CenterMap.IsEnabled = false;
            DuraText.Text = "";
            LenText.Text = "";
        }
        
        /// <summary>
        /// Validates the UI
        /// </summary>
        private void ValidateView() {
            DeleteAllButton.IsEnabled = true;
            GoButton.IsEnabled = true;
            ReverseWaypoints.IsEnabled = true;
            CenterMap.IsEnabled = true;
        }

        /// <summary>
        /// This method loads all the bookmarks and places them on the map
        /// </summary>
        public void LoadBookmarks() {
            SettingsManager manager = SettingsManager.GetDefaultSettingsManager();
            Bookmarks = manager.ReadList<Bookmark>("Bookmarks");
            if (Bookmarks == null) {
                Bookmarks = new List<Bookmark>();
                manager.WriteList<Bookmark>("Bookmarks", Bookmarks);
                return;
            }
            foreach (Bookmark bookmark in Bookmarks) {
                MapIcon icon = new MapIcon();
                icon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Icons/S.png"));
                icon.Title = bookmark.Name;
                Map.MapElements.Add(icon);
            }
        }

    }

    /// <summary>
    /// Represents a type of via point
    /// </summary>
    public enum ViaType {
        Start, End, Pivot
    }

    /// <summary>
    /// Represens a waypoint on a map.
    /// </summary>
    public class Waypoint : DependencyObject {
        /// <summary>
        /// The position of the waypoint
        /// </summary>
        public Geopoint Position { get; set; }
        /// <summary>
        /// The type of waypoint
        /// </summary>
        public ViaType Type { get; set; }
        /// <summary>
        /// The description of the waypoint
        /// </summary>
        public string Description {
            get {
                return String.Format("{0},{1}", Math.Round(Position.Position.Latitude, 4), Math.Round(Position.Position.Longitude, 4));
            }
        }
        /// <summary>
        /// The waypoint icon
        /// </summary>
        public MapIcon Icon { get; set; }
    }

    class Bookmark {
        public Geopoint Location { get; set; }
        public string Name { get; set; }
        [XmlIgnore] public MapIcon Icon { get; set; }
    }

}
