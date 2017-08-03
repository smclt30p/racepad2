using Racepad2.Route;
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

    }
}