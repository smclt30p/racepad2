/*
* Parsers - Copyright 2017 Supremacy Software
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
using System.Threading.Tasks;
using System.Xml;

using Windows.Devices.Geolocation;

using Racepad2.Core.Navigation.Route;
using Racepad2.Navigation.Maths;
using Windows.Storage;

namespace Racepad2.Core.Navigation.Parsers {

    class GPXRouteParser : RouteParser {

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
            List<Corner> corners = DriveRoute.ParseCorners(PolyUtil.simplify(path, 5));
            if (path.Count < 2) return route;
            route.Corners = corners;
            route.Path = path;
            return route;
        }
        
        public List<BasicGeoposition> ParsePath() {
            List<BasicGeoposition> positions = new List<BasicGeoposition>();
            XmlDocument document = new XmlDocument();
            document.InnerXml = innerData;
            XmlNode trkseg = document.GetElementsByTagName("trkseg")[0];
            if (trkseg == null) {
                throw new ParserException("GPX has no track!");
            }
            if (!trkseg.HasChildNodes) {
                throw new ParserException("GPX track is empty");
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
            if (positions.Count < 3) {
                throw new ParserException("GPX must have at lest 2 nodes!");
            }
            return positions;
        }
        
    }
    
    class ParserException : Exception {
        
        public ParserException(string reason) : base(reason) {
        }

    }
    
    abstract class RouteParser {

        public static async Task<DriveRoute> ParseRouteFromFileAsync(IStorageItem item, RouteFileType type) {
            
            switch (type) {
                case RouteFileType.GPX:
                    try {
                        string xml = await FileReader.ReadFile(item);
                        GPXRouteParser parser = new GPXRouteParser(xml);
                        return await parser.ParseAsync();
                    } catch (ParserException e) {
                        return null;
                    }

                default:
                    throw new ParserException("Unknown parse file");
            }

        }

        public abstract Task<DriveRoute> ParseAsync();
    }

    public enum RouteFileType {
        GPX
    }
}