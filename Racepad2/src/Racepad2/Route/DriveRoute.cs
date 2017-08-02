using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Racepad2.Geo.Navigation.Core
{
    class DriveRoute
    {
        public List<BasicGeoposition> Path { get; set; }
        public List<Corner> Corners { get; set; }
        public double EntranceBearing { get; set; }
        public CourseStatus Status { get; set; }
    }
}
