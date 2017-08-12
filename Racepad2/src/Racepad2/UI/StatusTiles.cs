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