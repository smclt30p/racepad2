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

    /// <summary>
    /// This class is used for imperial conversions and time conversion
    /// </summary>
    public class DisplayConvertor : IUnitConvertor {

        public static DisplayConvertor instance;

        public Units Units { get; private set; }
        public TimeFormat TimeFormat { get; private set; }

        /// <summary>
        /// Returns the unit converter singleton
        /// </summary>
        public static DisplayConvertor GetUnitConvertor() {
            if (instance == null) {
                instance = new DisplayConvertor();
            }
            return instance;
        }

        public DisplayConvertor() {
            LoadSettings();
        }

        /// <summary>
        /// Loads all the settings from the volatile memory
        /// </summary>
        private void LoadSettings() {
            SettingsManager manager = SettingsManager.GetDefaultSettingsManager();
            string units = manager.GetSetting("Units", "Metric");
            switch (units) {
                case "Metric":
                    this.Units = Units.Metric;
                    break;
                case "Imperial":
                    this.Units = Units.Imperial;
                    break;
            }
            string time = manager.GetSetting("TimeFormat", "H24");
            switch (time) {
                case "H24":
                    this.TimeFormat = TimeFormat.H24;
                    break;
                case "H12":
                    this.TimeFormat = TimeFormat.H12;
                    break;
            }
        }

        /// <summary>
        /// Returns the visual speed unit used for display.
        /// </summary>
        public string GetVisualSpeedUnit() {
            LoadSettings();
            switch (Units) {
                case Units.Imperial:
                    return "MPH";
                case Units.Metric:
                    return "km/h";
                default:
                    return "km/h";
            }
        }

        /// <summary>
        /// Returns the visual distance unit used for display.
        /// </summary>
        public string GetVisualDistanceUnit() {
            LoadSettings();
            switch (Units) {
                case Units.Imperial:
                    return "mi";
                case Units.Metric:
                    return "km";
                default:
                    return "km";
            }
        }

        /// <summary>
        /// Converts a metric distance to a imperial distance if needed
        /// </summary>
        public double ConvertDistance(double metric) {
            return ConvertSpeed(metric);
        }

        /// <summary>
        /// Converts a metric speed to a imperial speed if needed
        /// </summary>
        public double ConvertSpeed(double metric) {
            LoadSettings();
            if (Units == Units.Metric) return metric;
            return metric * 0.62137119223733d;
        }

        /// <summary>
        /// Convers metric altitude to imperial if needed
        /// </summary>
        public double ConvertAltitude(double metric) {
            LoadSettings();
            if (Units == Units.Metric) return metric;
            return metric * 3.280839895d;
        }

        /// <summary>
        /// Returns the visual altitude display unit, feet or meters
        /// </summary>
        /// <returns></returns>
        public string GetVisualAltitudeUnit() {
            LoadSettings();
            switch (Units) {
                case Units.Imperial:
                    return "ft";
                case Units.Metric:
                    return "m";
                default:
                    return "m";
            }
        }

        /// <summary>
        /// Returns the time format, 24h or 12h
        /// </summary>
        public string GetTimeFormat() {
            switch (TimeFormat) {
                case TimeFormat.H12:
                    return @"hh\:mm tt";
                case TimeFormat.H24:
                    return @"HH\:mm";
                default:
                    return @"HH\:mm";
            }
        }

        /// <summary>
        /// Returns the ISO timestamp format, either in 12h american or 24h european
        /// </summary>
        /// <returns></returns>
        public string GetTimestampFormat() {
            switch (TimeFormat) {
                case TimeFormat.H12:
                    return @"MM\/dd\/yyyy hh:mm:ss";
                case TimeFormat.H24:
                    return @"dd\/MM\/yyyy HH:mm:ss";
                default:
                    return @"dd\/MM\/yyyy HH:mm:ss";
            }
        }

        /// <summary>
        /// Returns the number of seconds elapsed since 1.1.1970
        /// </summary>
        public double CurrentTimeSeconds() {
            TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks);
            TimeSpan unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            return unixTicks.TotalSeconds;
        }

        /// <summary>
        /// Returns a timestamp with a custom format from seconds, unix time
        /// </summary>
        public string Timestamp(double seconds, string format) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan time = TimeSpan.FromSeconds((int)seconds);
            epoch = epoch + time;
            epoch = epoch.ToLocalTime();
            return epoch.ToString(format);
        }

        /// <summary>
        /// Returns a ISO timestamp from seconds, unix time
        /// </summary>
        public string ISOTimestamp(double seconds) {
            return Timestamp(seconds, GetTimestampFormat());
        }

    }

    public enum Units {
        Imperial, Metric
    }

    interface IUnitConvertor {
        /// <summary>
        /// Gets the visual speed unit (MPH or km/h)
        /// </summary>
        string GetVisualSpeedUnit();
        /// <summary>
        /// Gets the visual distance unit (km or miles)
        /// </summary>
        string GetVisualDistanceUnit();
        /// <summary>
        /// Normalizes the metric distance
        /// </summary>
        double ConvertDistance(double metric);
        /// <summary>
        /// Normalizes the metric speed
        /// </summary>
        double ConvertSpeed(double metric);
        double ConvertAltitude(double metric);
        string GetVisualAltitudeUnit();
        string GetTimeFormat();
        string GetTimestampFormat();
    }

    public enum TimeFormat {
        H24, H12 
    }

}
