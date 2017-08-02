using Racepad2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;

namespace Racepad2.Route {
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
            } set {
                _time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public string Instruction {
            get {
                return _insruction;
            } set {
                _insruction = value;
                NotifyPropertyChanged("Instruction");
            }
        }
         
        public double Distance {
            get {
                return _distance;
            } set {
                _distance = value;
                NotifyPropertyChanged("Distance");
            }
        }


        public double Elevation {
            get {
                return _elevation;
            }  set {
                this._elevation = value;
                NotifyPropertyChanged("Elevation");
            }
        }


        public double Speed {

            get {
                return _speed;
            } set {

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
            } set {
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
