/*
* StatusTiles - Copyright 2017 Supremacy Software
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

using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Racepad2.UI.StatusTiles {

    /// <summary>
    /// Represents a XAML control that can hold a 
    /// <see cref="BasicStatusTile"/>, loading it
    /// based on the class name. 
    /// </summary>
    class TileContainer : UserControl {

        private BasicStatusTile _innerTile;

        /// <summary>
        /// This is the class name of the tile that is loaded
        /// via reflection into the UI
        /// </summary>
        public string Tile {
            get {
                return _innerTile.GetType().ToString();
            }
            set {
                Type requestedType = Type.GetType(String.Format("Racepad2.UI.StatusTiles.{0}", value));
                if (requestedType == null) {
                    throw new Exception("Requested tile not found!");
                }
                _innerTile = (BasicStatusTile)Activator.CreateInstance(requestedType);
                if (_innerTile == null) {
                    throw new Exception("Tile instance is null!");
                }
                base.Content = _innerTile;
            }
        }

        /// <summary>
        /// This sets the status tile's raw value declaratively
        /// </summary>
        public object Value {
            get {
                if (base.Content != null && base.Content is BasicStatusTile) {
                    BasicStatusTile tile = (BasicStatusTile)base.Content;
                    return tile.Value;
                } else return null;
            }
            set {
                if (base.Content != null && base.Content is BasicStatusTile) {
                    BasicStatusTile tile = (BasicStatusTile)base.Content;
                    tile.Value = value;
                }
            }
        }
    }

    /// <summary>
    /// This presents a basic status tile with a small
    /// description and the master text inside of it.
    /// 
    /// Specific tiles are supposed to inherit this 
    /// class and add view conversion for raw values
    /// that are provided via the Value property of this
    /// class.
    /// </summary>
    abstract class BasicStatusTile : UserControl {

        private Grid Host { get; set; }
        private TextBlock _value;
        private TextBlock _description;

        /// <summary>
        /// This property holds the status tile's
        /// master value.
        /// </summary>
        public virtual object Value {
            get {
                return _value.Text;
            }
            set {
                _value.Text = value.ToString();
            }
        }

        /// <summary>
        /// This property holds the status tile's
        /// small description.
        /// </summary>
        public string BottomText {
            get {
                return _description.Text;
            }
            set {
                _description.Text = value;
            }
        }

        public BasicStatusTile() {
            InitializeComponent();
        }

        /// <summary>
        /// This initializes the basic status tile control hierarchy
        /// </summary>
        private void InitializeComponent() {
            Host = new Grid();
            RowDefinition top = new RowDefinition();
            RowDefinition bottom = new RowDefinition();
            GridLength topGrid = new GridLength(1, GridUnitType.Auto);
            GridLength bottomGrid = new GridLength(1, GridUnitType.Star);
            top.Height = topGrid;
            bottom.Height = bottomGrid;
            Host.RowDefinitions.Add(top);
            Host.RowDefinitions.Add(bottom);
            _description = new TextBlock() {
                Text = "Undefined",
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                TextAlignment = Windows.UI.Xaml.TextAlignment.Center,
                Margin = new Thickness(5)
            };
            FontWeight weight = new FontWeight();
            weight.Weight = 600;
            _value = new TextBlock() {
                Text = "NaN",
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = Windows.UI.Xaml.TextAlignment.Center,
                FontSize = 48,
                Margin = new Thickness(0, 20, 0, 0),
                FontWeight = weight
            };
            Host.Children.Insert(0, _description);
            Host.Children.Insert(1, _value);
            base.Content = Host;
            base.SizeChanged += BasicStatusTile_SizeChanged;
        }

        /// <summary>
        /// The tile's master text and description text get resized based
        /// on the screen's size. This handles that.
        /// </summary>
        private void BasicStatusTile_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e) {
            double third = Host.ActualHeight / 6;
            double width = Host.ActualWidth;
            _description.FontSize = third;
            if (_value.ActualWidth > width) {
                _value.FontSize = third * 2;
            } else {
                _value.FontSize = third * 3;
            }
        }
    }

    /// <summary>
    /// This is the altitude tile that displays the current
    /// altitude in meters
    /// </summary>
    class AltitudeTile : BasicStatusTile {
        public AltitudeTile() {
            BottomText = "Altitude (m)";
        }
    }

    /// <summary>
    /// This is the average speed tile that displays the 
    /// current average speed in a time span of 10 seconds.
    /// </summary>
    class AverageSpeedTile : BasicStatusTile {
        public AverageSpeedTile() : base() {
            BottomText = "Avg. Speed (10s)";
        }
        public override object Value {
            get {
                return base.Value;
            }
            set {
                double avgSpeed = Double.Parse(value.ToString()) * 3.6;
                base.Value = Convert.ToString(Math.Round(avgSpeed, 2));
            }
        }
    }

    /// <summary>
    /// This is the distance tile that displays the
    /// distance that has been driven.
    /// </summary>
    class DistanceTile : BasicStatusTile {
        public DistanceTile() {
            BottomText = "Distance (km)";
        }
        public override object Value {
            get {
                return base.Value.ToString();
            }
            set {
                double distanceMeters = Double.Parse(value.ToString());
                base.Value = Convert.ToString(Math.Round((distanceMeters / 1000), 2));
            }
        }
    }

    /// <summary>
    /// This is the max speed tile that represents the
    /// maximum speed in the current session
    /// </summary>
    class MaxSpeedTile : BasicStatusTile {
        public MaxSpeedTile() {
            BottomText = "Max. Speed (km/h)";
        }
        public override object Value {
            get {
                return base.Value;
            }
            set {
                double maxSpeed = Double.Parse(value.ToString()) * 3.6;
                base.Value = Convert.ToString(Math.Round(maxSpeed, 2));
            }
        }
    }

    /// <summary>
    /// This is the speed tile that represents the vehicles
    /// current moving speed.
    /// </summary>
    class SpeedTile : BasicStatusTile {
        public SpeedTile() : base() {
            base.BottomText = "Speed (km/h)";
        }
        public override object Value {
            get {
                return base.Value.ToString();
            }
            set {
                double speed = Double.Parse(value.ToString());
                base.Value = Convert.ToString(Math.Round(speed * 3.6, 2));
            }
        }
    }

    /// <summary>
    /// This is the time tile, this tile displays the time that has 
    /// passed since the session start.
    /// </summary>
    class TimeTile : BasicStatusTile {
        public TimeTile() {
            BottomText = "Duration";
        }
        public override object Value {
            get {
                return null;
            }
            set {
                if (value is int) {
                    base.Value = value.ToString();
                    TimeSpan time = TimeSpan.FromSeconds((int)value);
                    base.Value = time.ToString(@"hh\:mm\:ss");
                }
            }
        }
    }
}