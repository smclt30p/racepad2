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
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.IO;

namespace Racepad2.Core.Navigation.Route {

    public class Corner {

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
    public enum CornerType {
        LEFT_HAIRPIN,
        LEFT_TWO,
        LEFT_THREE,
        RIGHT_HAIRPIN,
        RIGHT_TWO,
        RIGHT_THREE,
        STRAIGHT,
        UNKNOWN,
    }

    public class DriveRoute {

        public IReadOnlyList<BasicGeoposition> Path { get; set; }
        public IReadOnlyList<Corner> Corners { get; set; }
        public CourseStatus Status { get; set; }

        /// <summary>
        /// Splits the given route into vectors for slope calculation.
        /// </summary>
        /// <param name="Route">The path to be split</param>
        /// <returns>A list of vectors from the route</returns>
        public static List<GeopositionVector> GetVectorsFromRoute(Geopath path) {
            IReadOnlyList<BasicGeoposition> Route = path.Positions;
            List<GeopositionVector> pairs = new List<GeopositionVector>();
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
                GeopositionVector pair = new GeopositionVector();
                pair.Path.Add(geo1);
                pair.Path.Add(geo2);
                pair.SlopePercentage = percentage;
                pairs.Add(pair);
            }
            return pairs;
        }

        /// <summary>
        /// Returns all the corners from the route
        /// </summary>
        public static IReadOnlyList<Corner> ParseCorners(IReadOnlyList<BasicGeoposition> positions) {
            List<Corner> corners = new List<Corner>();
            for (int i = 0; i < positions.Count; i++) {
                if (i + 3 > positions.Count) break;
                double angle = GeoMath.Angle(positions[i], positions[i + 1], positions[i + 2]);
                CornerType type = GetCornerType(angle);
                if (type == CornerType.STRAIGHT) continue;
                Corner corner = new Corner();
                corner.Position = positions[i + 1];
                corner.CornerType = type;
                corner.Angle = angle;
                corner.EntranceBearing = GeoMath.Bearing(positions[i], positions[i + 1]);
                corner.ExitEaring = GeoMath.Bearing(positions[i + 1], positions[i + 2]);
                corners.Add(corner);
            }
            return corners;
        }

        /// <summary>
        /// Returns the type of the corner based on angle
        /// </summary>
        private static CornerType GetCornerType(double angle) {
            if (angle >= 0 && angle <= 45) return CornerType.RIGHT_HAIRPIN;
            if (angle >= 45 && angle <= 90) return CornerType.RIGHT_TWO;
            if (angle >= 90 && angle <= 145) return CornerType.RIGHT_THREE;
            if (angle >= 145 && angle <= 215) return CornerType.STRAIGHT;
            if (angle >= 215 && angle <= 270) return CornerType.LEFT_THREE;
            if (angle >= 270 && angle <= 315) return CornerType.LEFT_TWO;
            if (angle >= 315 && angle <= 360) return CornerType.LEFT_HAIRPIN;
            return CornerType.UNKNOWN;
        }

        /// <summary>
        /// Calculates the average slope from a path
        /// </summary>
        /// <param name="path">The path to be calculated</param>
        /// <returns>The average slope in percent</returns>
        public static double GetAverageSlope(Geopath path) {
            List<GeopositionVector> gradients = GetVectorsFromRoute(path);
            double avgslope = 0;
            foreach (GeopositionVector pair in gradients) {
                avgslope += pair.SlopePercentage;
            }
            return Math.Round(avgslope / gradients.Count, 0);
        }
        
        /// <summary>
        /// Returns the length of a given path.
        /// </summary>
        /// <param name="route">The route to calculate the distance for</param>
        /// <returns>The distance in meters</returns>
        public static double GetLength(DriveRoute route) {
            IReadOnlyList<BasicGeoposition> Route = route.Path;
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

        /// <summary>
        /// Get a color from a percentage slope. This is used to
        /// visualize slopes on a map.
        /// </summary>
        public static Color GetColorFromSlope(double percentage) {
            if (percentage < -20) return GetSolidColorBrush("0000FF");
            if (percentage > 20) return GetSolidColorBrush("FF0000");
            if (percentage >= -20 && percentage <= -15) return GetSolidColorBrush("00fffF");
            if (percentage >= -15 && percentage <= -10) return GetSolidColorBrush("0060ff");
            if (percentage >= -10 && percentage <= -5) return GetSolidColorBrush("00ff12");
            if (percentage >= -5 && percentage <= 0) return GetSolidColorBrush("00ff12");
            if (percentage >= 0 && percentage <= 5) return GetSolidColorBrush("fffc00");
            if (percentage >= 5 && percentage <= 10) return GetSolidColorBrush("fffc00");
            if (percentage >= 10 && percentage <= 15) return GetSolidColorBrush("ff9600");
            if (percentage >= 15 && percentage <= 20) return GetSolidColorBrush("ff00f0");
            return Colors.Purple;
        }

        /// <summary>
        /// Returns a color from a hex color description format, for ex: #FF00FF
        /// </summary>
        public static Color GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = 255;
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush.Color;
        }
    }

    /// <summary>
    /// Represents a vector if 2 geopositions with a height difference
    /// between them.
    /// </summary>
    public class GeopositionVector {
        public List<BasicGeoposition> Path { get; set; } = new List<BasicGeoposition>();
        public double SlopePercentage { get; set; }
    }

    public enum CourseStatus {
        COURSE_STARTED,
        COURSE_PAUSED,
        COURSE_LAST_STRAIGHT,
        COURSE_FINISHED,
        COURSE_NOT_STARTED,
        COURSE_IN_PROGRESS,
        COURSE_OFF_COURSE
    }

    /// <summary>
    /// This class represents a active session that is beign ridden.
    /// </summary>
    public class Session : INotifyPropertyChanged {

        /// <summary>
        /// The name of the session
        /// </summary>
        [XmlAttribute] public string Name { get; set; }
        /// <summary>
        /// The current heading of the vehicle
        /// </summary>
        [XmlIgnore] public double Heading { get; set; }
       
        /// <summary>
        /// The maximum speed archived in the session
        /// </summary>
        public double MaxSpeed { get; set; }

        
        [XmlIgnore] private WalkingList<double> _speedList = new WalkingList<double>();
        [XmlIgnore] private List<BasicGeoposition> _path = new List<BasicGeoposition>();
        [XmlIgnore] private List<double> _elevationProfile = new List<double>();
        [XmlIgnore] private VehiclePosition _currentPosition;
        [XmlIgnore] private double _speed = 0; // m/s
        [XmlIgnore] private double _elevation = 0; // m
        [XmlIgnore] private double _averageSpeed = 0; // m/s
        [XmlIgnore] private double _distance = 0; // m
        [XmlIgnore] private string _insruction = "";
        [XmlIgnore] private int _time = 0;

        /// <summary>
        /// The ridden path of the session
        /// </summary>
        public List<BasicGeoposition> Path {
            get {
                return _path; 
            } set {
                this.Path = _path;
            }
        }

        /// <summary>
        /// The current position of the vehicle
        /// </summary>
        [XmlIgnore] public VehiclePosition CurrentPosition {
            get {
                if (_currentPosition == null) _currentPosition = new VehiclePosition();
                return _currentPosition;
            } set {
                _currentPosition = value;
                _path.Add(value.Position.Position);
                NotifyPropertyChanged("CurrentPosition");
            }
        }

        /// <summary>
        /// The time in seconds from the start of the session
        /// </summary>
        public int Time {
            get {
                return _time;
            }
            set {
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

        /// <summary>
        /// The current visual instruction on the screen
        /// </summary>
        [XmlIgnore] public string Instruction {
            get {
                return _insruction;
            }
            set {
                _insruction = value;
                NotifyPropertyChanged("Instruction");
            }
        }

        /// <summary>
        /// The ridden distance in meters from the start
        /// </summary>
        public double Distance {
            get {
                return _distance;
            }
            set {
                _distance = value;
                NotifyPropertyChanged("Distance");
            }
        }

        /// <summary>
        /// The current elevation
        /// </summary>
        [XmlIgnore] public double Elevation {
            get {
                return _elevation;
            }
            set {
                this._elevationProfile.Add(value);
                this._elevation = value;
                NotifyPropertyChanged("Elevation");
            }
        }

        /// <summary>
        /// The current speed
        /// </summary>
        [XmlIgnore] public double Speed {
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

        /// <summary>
        /// The weighted average speed
        /// </summary>
        public double AverageSpeed {
            get {
                return Math.Round(_averageSpeed, 2);
            }
            set {
                _averageSpeed = value;
                NotifyPropertyChanged("AverageSpeed");
            }
        }

        /// <summary>
        /// Screen updating housekeeping
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyname) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        /// <summary>
        /// This recalculates the average speed from a speed snapshot. 
        /// There is a list of speeds where the average gets calculated.
        /// The weighting duration is 10 seconds.
        /// </summary>
        /// <param name="value"></param>
        private void RecalcAverageSpeed(double value) {
            _speedList.Add(value);
            double avg = 0;
            foreach (double item in _speedList) {
                if (item != 0) avg += item;
            }
            _averageSpeed = avg / _speedList.Count;
            NotifyPropertyChanged("AverageSpeed");
        }

        /// <summary>
        /// Serialize a given session to a string for storage
        /// </summary>
        public static string Serialize(Session session) {
            XmlSerializer serializer = new XmlSerializer(typeof(Session));
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, session);
            return writer.ToString();
        }

        /// <summary>
        /// Deserialize a data object into a session from storage
        /// </summary>
        public static Session Deserialize(string data) {
            XmlSerializer serializer = new XmlSerializer(typeof(Session));
            StringReader reader = new StringReader(data);
            return (Session) serializer.Deserialize(reader);
        }

    }

    /// <summary>
    /// This class describes a vehicle position on the map.
    /// </summary>
    public class VehiclePosition {

        public VehiclePosition() {
        }

        public VehiclePosition(Geopoint position, double? bearing) {
            Position = position;
            Bearing = bearing;
        }

        public Geopoint Position { get; set; }
        public double? Bearing { get; set; }

    }

}