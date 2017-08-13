﻿/*
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
using System.Diagnostics;

using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Storage.Streams;

using Racepad2.UI.Controls;


namespace Racepad2 {

    /// <summary>
    /// A page that contains a few navigation elements and the general map
    /// </summary>
    public sealed partial class MapViewPage : Page {

        /// <summary>
        /// The main deciding factor of the whole planner, the
        /// waypoints that have been placed on the map.
        /// </summary>
        public ObservableCollection<Waypoint> Waypoints { get; set; }

        public MapViewPage() {
            this.InitializeComponent();
            Waypoints = new ObservableCollection<Waypoint>();
            Waypoints.CollectionChanged += Waypoints_CollectionChanged;
            Map.ToPointSelected += Map_ToPointSelected;
            Map.FromPointSelected += Map_FromPointSelected;
            Map.PivotPointSelected += Map_PivotPointSelected;
        }

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
                MapRouteFinderResult result = await MapRouteFinder.GetDrivingRouteFromWaypointsAsync(path);
                Progress.Visibility = Visibility.Collapsed;
                if (result != null) {
                    Map.Route = result.Route.Path;
                } else {
                    Debug.WriteLine("Nothing found!");
                }
            } else {
                Map.Route = null;
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

}
