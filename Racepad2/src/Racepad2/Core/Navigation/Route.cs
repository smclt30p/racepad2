/*
* Route - Copyright 2017 Supremacy Software
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
using System.ComponentModel;

using Windows.Devices.Geolocation;

using Racepad2.Navigation.Maths;


namespace Racepad2.Core.Navigation.Route {

    class Corner {

        public CornerType CornerType { get; set; }
        public BasicGeoposition Position { get; set; }
        public double Angle { get; set; }
        public double EntranceBearing { get; set; }
        public double ExitEaring { get; set; }

        /// <summary>
        /// Debug information for the corner
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return CornerType.ToString() + " - Lat: " + Position.Latitude +
                " - Long: " + Position.Longitude + " - Angle: " + Angle;
        }

        /// <summary>
        /// Get the visual corner description
        /// </summary>
        /// <param name="corner">The corner </param>
        /// <returns>Textual representation of the corner</returns>
        public static string GetDescriptionForCorner(Corner corner) {
            switch (corner.CornerType) {
                case CornerType.LEFT_HAIRPIN:
                    return "hairpin left";
                case CornerType.LEFT_THREE:
                    return "left";
                case CornerType.LEFT_TWO:
                    return "square left";
                case CornerType.RIGHT_HAIRPIN:
                    return "hairpin right";
                case CornerType.RIGHT_THREE:
                    return "right";
                case CornerType.RIGHT_TWO:
                    return "square right";
            }
            return "Unknown";
        }
    }

    /// <summary>
    /// Enumeration describing the various types of corners detected, used for icons
    /// </summary>
    enum CornerType {
        LEFT_HAIRPIN,
        LEFT_TWO,
        LEFT_THREE,
        RIGHT_HAIRPIN,
        RIGHT_TWO,
        RIGHT_THREE,
        STRAIGHT,
        UNKNOWN,
    }

    class DriveRoute {

        public List<BasicGeoposition> Path { get; set; }
        public List<Corner> Corners { get; set; }
        public double EntranceBearing { get; set; }
        public CourseStatus Status { get; set; }

        public static List<GradientPair> GetGradientPairsFromRoute(List<BasicGeoposition> Route) {
            List<GradientPair> pairs = new List<GradientPair>();
            BasicGeoposition geo1;
            BasicGeoposition geo2;
            for (int i = 0; ; i++) {
                if (i + 1 >= Route.Count) break;
                geo1 = Route[i];
                geo2 = Route[i + 1];
                double run = GeoMath.Distance(geo1, geo2);
                double rise = geo2.Altitude - geo1.Altitude;
                double percentage = rise / run * 100;
                geo1.Altitude = 0;
                geo2.Altitude = 0;
                GradientPair pair = new GradientPair();
                pair.Path.Add(geo1);
                pair.Path.Add(geo2);
                pair.SlopePercentage = percentage;
                pairs.Add(pair);
            }
            return pairs;
        }


        public static double GetAverageSlope(List<BasicGeoposition> path) {
            List <GradientPair> gradients = GetGradientPairsFromRoute(path);
            double avgslope = 0;
            foreach (GradientPair pair in gradients) {
                avgslope += pair.SlopePercentage;
            }
            return Math.Round(avgslope / gradients.Count, 0);
        }

        public static double GetLength(DriveRoute route) {
            List<BasicGeoposition> Route = route.Path;
            BasicGeoposition geo1;
            BasicGeoposition geo2;
            double distance = 0;
            for (int i = 0; ; i++) {
                if (i + 1 >= Route.Count) break;
                geo1 = Route[i];
                geo2 = Route[i + 1];
                double run = GeoMath.Distance(geo1, geo2);
                distance += run;
            }
            return distance;
        }
    }

    class GradientPair {
        public List<BasicGeoposition> Path { get; set; } = new List<BasicGeoposition>();
        public double SlopePercentage { get; set; }
    }

    enum CourseStatus {
        COURSE_STARTED,
        COURSE_PAUSED,
        COURSE_LAST_STRAIGHT,
        COURSE_FINISHED,
        COURSE_NOT_STARTED,
        COURSE_IN_PROGRESS,
        COURSE_OFF_COURSE
    }

    class Ride : INotifyPropertyChanged {

        public string Name { get; set; }
        public double Heading { get; set; }
        public double Position { get; set; }
        public BasicGeoposition Start { get; set; }
        public List<BasicGeoposition> Path { get; set; }
        public double MaxSpeed { get; set; }

        private WalkingList<double> _speedList = new WalkingList<double>();
        private double _speed = 0; // m/s
        private double _elevation = 0; // m
        private double _averageSpeed = 0; // m/s
        private double _distance = 0; // m
        private string _insruction = "";
        private int _time = 0;

        public int Time {
            get {
                return _time;
            }
            set {
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public string Instruction {
            get {
                return _insruction;
            }
            set {
                _insruction = value;
                NotifyPropertyChanged("Instruction");
            }
        }

        public double Distance {
            get {
                return _distance;
            }
            set {
                _distance = value;
                NotifyPropertyChanged("Distance");
            }
        }

        public double Elevation {
            get {
                return _elevation;
            }
            set {
                this._elevation = value;
                NotifyPropertyChanged("Elevation");
            }
        }

        public double Speed {
            get {
                return _speed;
            }
            set {
                _speed = value;
                RecalcAverageSpeed(_speed);
                if (_speed > MaxSpeed) {
                    MaxSpeed = _speed;
                    NotifyPropertyChanged("MaxSpeed");
                }
                NotifyPropertyChanged("Speed");
            }
        }

        public double AverageSpeed {
            get {
                return Math.Round(_averageSpeed, 2);
            }
            set {
                _averageSpeed = value;
                NotifyPropertyChanged("AverageSpeed");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyname) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        private void RecalcAverageSpeed(double value) {
            _speedList.Add(value);
            double avg = 0;
            foreach (double item in _speedList) {
                if (item != 0) avg += item;
            }
            _averageSpeed = avg / _speedList.Count;
            NotifyPropertyChanged("AverageSpeed");
        }
    }
}