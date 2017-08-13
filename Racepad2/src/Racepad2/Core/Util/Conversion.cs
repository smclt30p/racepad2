/*
* Conversion - Copyright 2017 Supremacy Software
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

namespace Racepad2.Core.Util.Conversions {

    class TimeConversions {
        
        /// <summary>
        /// Returns the number of seconds elapsed since 1.1.1970
        /// </summary>
        public static double CurrentTimeSeconds() {
            TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks);
            TimeSpan unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            return unixTicks.TotalSeconds;
        }

        /// <summary>
        /// Returns a timestamp with a custom format from seconds, unix time
        /// </summary>
        public static string Timestamp(double seconds, string format) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan time = TimeSpan.FromSeconds((int)seconds);
            epoch = epoch + time;
            epoch = epoch.ToLocalTime();
            return epoch.ToString(format);
        }

        /// <summary>
        /// Returns a ISO timestamp from seconds, unix time
        /// </summary>
        public static string ISOTimestamp(double seconds) {
            return Timestamp(seconds, @"dd\/MM\/yyyy HH:mm:ss");
        }

    }

}
