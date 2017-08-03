using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace Racepad2.Route {
    class GradientPair {
        public List<BasicGeoposition> Path { get; set; } = new List<BasicGeoposition>();
        public double SlopePercentage { get; set; }
    }
}
