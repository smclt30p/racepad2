using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.Devices.Geolocation;

using Racepad2.Geo.Navigation.Core;
using System.Diagnostics;

namespace Racepad2.Geo.Navigation
{
    class GPXRouteParser : RouteParser
    {

        private string innerData = null;

        public GPXRouteParser(string data) {
            this.innerData = data;
        }

        public override async Task<DriveRoute> ParseAsync() {
            return await Task.Run(() => Parse());
        }

        private DriveRoute Parse() {

            DriveRoute route = new DriveRoute();

            List<BasicGeoposition> path = ParsePath();
            List<Corner> corners = ParseCorners(path);

            if (path.Count < 2) return route;

            double entranceBearing = GeoMath.Bearing(path[0], path[1]);

            route.Corners = corners;
            route.Path = path;
            route.EntranceBearing = entranceBearing;

            return route;

        }

        public List<BasicGeoposition> ParsePath() {

            List<BasicGeoposition> positions = new List<BasicGeoposition>();

            XmlDocument document = new XmlDocument();
            document.InnerXml = innerData;

            XmlNode trkseg = document.GetElementsByTagName("trkseg")[0];

            if (trkseg == null) {
                throw new GPXException("GPX has no track!");
            }

            if (!trkseg.HasChildNodes) {
                throw new GPXException("GPX track is empty");
            }

            foreach (XmlElement node in trkseg.ChildNodes) {

                BasicGeoposition geo = new BasicGeoposition();

                double lat = Double.Parse(node.GetAttribute("lat"));
                double lon = Double.Parse(node.GetAttribute("lon"));

                geo.Latitude = lat;
                geo.Longitude = lon;

                foreach (XmlElement element in node.ChildNodes) {
                    if (element.Name == "ele") {
                        geo.Altitude = Double.Parse(element.InnerText);
                    }
                }
                
                positions.Add(geo);

            }

            return positions;

        }

        public List<Corner> ParseCorners(List<BasicGeoposition> positions) {

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

        

        
    }
}
