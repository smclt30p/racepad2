/*
* MapColorController - Copyright 2017 Supremacy Software
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
using Windows.UI.Xaml.Controls.Maps;

namespace Racepad2.UI.Maps {

    /// <summary>
    /// class used for color scheme switchting inside the navigation map view
    /// </summary>
    class MapColorController {

        /// <summary>
        /// Controller is singleton
        /// </summary>
        private static MapColorController instance;
        public DispatcherTimer ColorChangeTimer { get; }
        public static MapColorController GetMapColorController() {
            if (instance == null) {
                instance = new MapColorController();
            }
            return instance;
        }

        /// <summary>
        /// The event that gets fired each 15 minutes to check the time
        /// and set the map color scheme
        /// </summary>
        public delegate void MapSchemeChanged(MapColorScheme scheme);
        public event MapSchemeChanged MapSchemeChanging;
        public void MapSchemeChange(MapColorScheme scheme) {
            if (MapSchemeChanging == null) return;
            MapSchemeChanging(scheme);
        }

        private MapColorController() {
            ColorChangeTimer = new DispatcherTimer();
            ColorChangeTimer.Interval = new TimeSpan(0, 15, 0);
            ColorChangeTimer.Tick += ColorChangeTimer_Tick;
            ColorChangeTimer.Start();
        }

        /// <summary>
        /// Timer tick, check time and emit event if needed
        /// </summary>
        private void ColorChangeTimer_Tick(object sender, object e) {
            if (IsNight()) {
                MapSchemeChanging(MapColorScheme.Dark);
            } else {
                MapSchemeChanging(MapColorScheme.Light);
            }
               
        }

        /// <summary>
        /// Method used to set the initial map color scheme
        /// </summary>
        internal MapColorScheme PulseOnce() {
            if (IsNight()) return MapColorScheme.Dark;
            return MapColorScheme.Light;
        }

        /// <summary>
        /// Returns true if the current time is between 20:00 and 6:00
        /// </summary>
        private bool IsNight() {
            DateTime time = DateTime.Now;
            int hour = Int32.Parse(time.ToString("HH"));
            return (hour >= 0 && hour <= 6) || (hour >= 20 && hour <= 24);
        }
    }
}
