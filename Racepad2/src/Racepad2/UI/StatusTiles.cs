﻿/*
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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Racepad2.Core.Util.Conversions;


namespace Racepad2.UI.StatusTiles {

    /// <summary>
    /// Represents a XAML control that can hold a 
    /// <see cref="BasicStatusTile"/>, loading it
    /// based on the class name. 
    /// </summary>
    class TileContainer : UserControl {

        private BasicStatusTile _innerTile;

        public TileType TileType {
            private get {
                return TileType.Normal;
            } set {
                switch (value) {
                    case TileType.Normal:
                        _innerTile.FontSizeOverride = 48;
                        break;
                    case TileType.Large:
                        _innerTile.FontSizeOverride = 100;
                        break;
                }
            }
        }

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
            BottomText = String.Format("Altitude ({0})", DisplayConvertor.GetUnitConvertor().GetVisualAltitudeUnit());
        }
        public override object Value {
            get {
                return base.Value;
            } set {
                double alt = DisplayConvertor.GetUnitConvertor().ConvertAltitude(Double.Parse(value.ToString()));
                base.Value = Convert.ToString(Math.Round(alt, 0));
            }
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
                double avgSpeed = DisplayConvertor.GetUnitConvertor().ConvertSpeed(Double.Parse(value.ToString()) * 3.6);
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
            BottomText = String.Format("Distance ({0})", DisplayConvertor.GetUnitConvertor().GetVisualDistanceUnit());
        }
        public override object Value {
            get {
                return base.Value.ToString();
            }
            set {
                double distanceMeters = DisplayConvertor.GetUnitConvertor().ConvertDistance(Double.Parse(value.ToString()));
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
            BottomText = String.Format("Max. Speed ({0})", DisplayConvertor.GetUnitConvertor().GetVisualSpeedUnit());
        }
        public override object Value {
            get {
                return base.Value;
            }
            set {
                double maxSpeed = DisplayConvertor.GetUnitConvertor().ConvertSpeed(Double.Parse(value.ToString()) * 3.6);
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
            base.BottomText = String.Format("Speed ({0})", DisplayConvertor.GetUnitConvertor().GetVisualSpeedUnit());
        }
        public override object Value {
            get {
                return base.Value.ToString();
            }
            set {
                double speed = DisplayConvertor.GetUnitConvertor().ConvertSpeed(Double.Parse(value.ToString()));
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

    class TimeOfDayTile : BasicStatusTile {

        private DispatcherTimer DispatcherTimer { get; set; }

        public TimeOfDayTile() {
            BottomText = "Time of day";
            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Interval = new TimeSpan(0, 1, 0);
            DispatcherTimer.Tick += DispatcherTimer_Tick;
            DispatcherTimer.Start();
            // tick the event once manually to update the UI once
            DispatcherTimer_Tick(null, null);
        }

        private void DispatcherTimer_Tick(object sender, object e) {
            DateTime time = DateTime.Now;
            Value = time.ToString(DisplayConvertor.GetUnitConvertor().GetTimeFormat());
        }
    }

    public enum TileType {
        Normal, Large
    }
}