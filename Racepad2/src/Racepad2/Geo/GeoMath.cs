/**
 * This is a class featuring geographic tools, such as angle between three 
 * goegraphic points and distance between two points. It has been implemented
 * on algorithms documented here: http://www.movable-type.co.uk/scripts/latlong.html
*/

using System;
using Windows.Devices.Geolocation;

namespace Racepad2.Geo
{
    class GeoMath
    {

        public static double Angle(BasicGeoposition arc1, BasicGeoposition pivot, BasicGeoposition arc2) {
            double angle1 = Bearing(pivot, arc1);
            double angle2 = Bearing(pivot, arc2);
            double absangle = ((angle1 - angle2) + 360) % 360;
            return absangle;
        }

        public static double Distance(BasicGeoposition pos1, BasicGeoposition pos2) {

            double lat1deg = pos1.Latitude;
            double lon1deg = pos1.Longitude;

            double lat2deg = pos2.Latitude;
            double lon2deg = pos2.Longitude;

            double earthDiameter = 6371e3; // meters

            /* φ1 */
            double lat1rad = DegreeToRadian(lat1deg);
            /* φ2 */
            double lat2rad = DegreeToRadian(lat2deg);

            /* Δφ */
            double deltalat = DegreeToRadian(lat2deg - lat1deg);
            /* Δλ */
            double deltalon = DegreeToRadian(lon2deg - lon1deg);

            double a = Math.Sin(deltalat / 2) * Math.Sin(deltalat / 2) +
                Math.Cos(lat1rad) * Math.Cos(lat2rad) *
                Math.Sin(deltalon / 2) * Math.Sin(deltalon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double d = earthDiameter * c;

            return d;

        }

        public static double Bearing(BasicGeoposition pos1, BasicGeoposition pos2) {

            double φ2 = pos2.Latitude;
            double φ1 = pos1.Latitude;

            double λ2 = pos2.Longitude;
            double λ1 = pos1.Longitude;

            double y = Math.Sin(λ2 - λ1) * Math.Cos(φ2);
            double x = Math.Cos(φ1) * Math.Sin(φ2) -
                    Math.Sin(φ1) * Math.Cos(φ2) * Math.Cos(λ2 - λ1);
            double brng = RadianToDegree(Math.Atan2(y, x));

            return (brng + 360) % 360;

        }

        private static double DegreeToRadian(double angle) {
            return Math.PI * angle / 180.0;
        }

        private static double RadianToDegree(double angle) {
            return angle * (180 / Math.PI);
        }

    }
}
