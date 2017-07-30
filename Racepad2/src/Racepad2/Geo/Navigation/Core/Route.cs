using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace Racepad2.Geo.Navigation.Core
{
    class Route
    {
        public List<BasicGeoposition> Path { get; set; }
        public ICollection<Corner> Corners { get; set; }
        public double EntranceBearing { get; set; }
    }
}
