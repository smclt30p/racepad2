/*
* Copyright 2008, 2013 Google Inc.
* Ported to UWP by Ognjen Galić, Cоpyright 2017
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
using System.Text;
using Windows.Devices.Geolocation;

namespace Racepad2.Geo.Google {






    public class PolyUtil {

        private PolyUtil() { }

        /* start c# layer */

        private static double PI = Math.PI;

        private static double toRadians(double angle) {
            return Math.PI * angle / 180.0;
        }

        private static double tan(double angle) {
            return Math.Tan(angle);
        }

        private static double sin(double angle) {
            return Math.Sin(angle);
        }

        private static double log(double number) {
            return Math.Log(number);
        }

        private static double mercator(double lat) {
            return log(tan(lat * 0.5 + PI / 4));
        }

        private static double inverseMercator(double y) {
            return 2 * Math.Atan(Math.Exp(y)) - PI / 2;
        }

        private static double wrap(double n, double min, double max) {
            return (n >= min && n < max) ? n : (mod(n - min, max - min) + min);
        }

        private static double hav(double x) {
            double sinHalf = sin(x * 0.5);
            return sinHalf * sinHalf;
        }

        private static double max(double inp, double inp2) {
            return Math.Max(inp, inp2);
        }

        private static double mod(double x, double m) {
            return ((x % m) + m) % m;
        }

        private static double clamp(double x, double low, double high) {
            return x < low ? low : (x > high ? high : x);
        }

        private static double min(double a, double b) {
            return Math.Min(a, b);
        }

        private static double cos(double a) {
            return Math.Cos(a);
        }

        static double havDistance(double lat1, double lat2, double dLng) {
            return hav(lat1 - lat2) + hav(dLng) * cos(lat1) * cos(lat2);
        }

        static double sqrt(double num) {
            return Math.Sqrt(num);
        }

        static double sinFromHav(double h) {
            return 2 * sqrt(h * (1 - h));
        }

        // Returns hav(asin(x)).
        static double havFromSin(double x) {
            double x2 = x * x;
            return x2 / (1 + sqrt(1 - x2)) * .5;
        }

        static double sinSumFromHav(double x, double y) {
            double a = sqrt(x * (1 - x));
            double b = sqrt(y * (1 - y));
            return 2 * (a + b - 2 * (a * y + b * x));
        }

        static double asin(double a) {
            return Math.Asin(a);
        }

        /**
         * Computes inverse haversine. Has good numerical stability around 0.
         * arcHav(x) == acos(1 - 2 * x) == 2 * asin(sqrt(x)).
         * The argument must be in [0, 1], and the result is positive.
         */
        static double arcHav(double x) {
            return 2 * asin(sqrt(x));
        }

        /* End C# layer */

        /**
         * Returns tan(latitude-at-lng3) on the great circle (lat1, lng1) to (lat2, lng2). lng1==0.
         * See http://williams.best.vwh.net/avform.htm .
         */
        private static double tanLatGC(double lat1, double lat2, double lng2, double lng3) {
            return (tan(lat1) * sin(lng2 - lng3) + tan(lat2) * sin(lng3)) / sin(lng2);
        }

        /**
         * Returns mercator(latitude-at-lng3) on the Rhumb line (lat1, lng1) to (lat2, lng2). lng1==0.
         */
        private static double mercatorLatRhumb(double lat1, double lat2, double lng2, double lng3) {
            return (mercator(lat1) * (lng2 - lng3) + mercator(lat2) * lng3) / lng2;
        }

        /**
         * Computes whether the vertical segment (lat3, lng3) to South Pole intersects the segment
         * (lat1, lng1) to (lat2, lng2).
         * Longitudes are offset by -lng1; the implicit lng1 becomes 0.
         */
        private static bool intersects(double lat1, double lat2, double lng2,
                                          double lat3, double lng3, bool geodesic) {
            // Both ends on the same side of lng3.
            if ((lng3 >= 0 && lng3 >= lng2) || (lng3 < 0 && lng3 < lng2)) {
                return false;
            }
            // Point is South Pole.
            if (lat3 <= -PI / 2) {
                return false;
            }
            // Any segment end is a pole.
            if (lat1 <= -PI / 2 || lat2 <= -PI / 2 || lat1 >= PI / 2 || lat2 >= PI / 2) {
                return false;
            }
            if (lng2 <= -PI) {
                return false;
            }
            double linearLat = (lat1 * (lng2 - lng3) + lat2 * lng3) / lng2;
            // Northern hemisphere and point under lat-lng line.
            if (lat1 >= 0 && lat2 >= 0 && lat3 < linearLat) {
                return false;
            }
            // Southern hemisphere and point above lat-lng line.
            if (lat1 <= 0 && lat2 <= 0 && lat3 >= linearLat) {
                return true;
            }
            // North Pole.
            if (lat3 >= PI / 2) {
                return true;
            }
            // Compare lat3 with latitude on the GC/Rhumb segment corresponding to lng3.
            // Compare through a strictly-increasing function (tan() or mercator()) as convenient.
            return geodesic ?
                tan(lat3) >= tanLatGC(lat1, lat2, lng2, lng3) :
                mercator(lat3) >= mercatorLatRhumb(lat1, lat2, lng2, lng3);
        }

        public static bool containsLocation(BasicGeoposition point, List<BasicGeoposition> polygon, bool geodesic) {
            return containsLocation(point.Latitude, point.Longitude, polygon, geodesic);
        }

        /**
         * Computes whether the given point lies inside the specified polygon.
         * The polygon is always considered closed, regardless of whether the last point equals
         * the first or not.
         * Inside is defined as not containing the South Pole -- the South Pole is always outside.
         * The polygon is formed of great circle segments if geodesic is true, and of rhumb
         * (loxodromic) segments otherwise.
         */
        public static bool containsLocation(double latitude, double longitude, List<BasicGeoposition> polygon, bool geodesic) {
            int size = polygon.Count;
            if (size == 0) {
                return false;
            }
            double lat3 = toRadians(latitude);
            double lng3 = toRadians(longitude);
            BasicGeoposition prev = polygon[size - 1];
            double lat1 = toRadians(prev.Latitude);
            double lng1 = toRadians(prev.Longitude);
            int nIntersect = 0;
            foreach (BasicGeoposition point2 in polygon) {
                double dLng3 = wrap(lng3 - lng1, -PI, PI);
                // Special case: point equal to vertex is inside.
                if (lat3 == lat1 && dLng3 == 0) {
                    return true;
                }
                double lat2 = toRadians(point2.Latitude);
                double lng2 = toRadians(point2.Longitude);
                // Offset longitudes by -lng1.
                if (intersects(lat1, lat2, wrap(lng2 - lng1, -PI, PI), lat3, dLng3, geodesic)) {
                    ++nIntersect;
                }
                lat1 = lat2;
                lng1 = lng2;
            }
            return (nIntersect & 1) != 0;
        }

        private static double DEFAULT_TOLERANCE = 0.1;  // meters.
        private static double EARTH_RADIUS = 6371009;

        /**
         * Computes whether the given point lies on or near the edge of a polygon, within a specified
         * tolerance in meters. The polygon edge is composed of great circle segments if geodesic
         * is true, and of Rhumb segments otherwise. The polygon edge is implicitly closed -- the
         * closing segment between the first point and the last point is included.
         */
        public static bool isLocationOnEdge(BasicGeoposition point, List<BasicGeoposition> polygon, bool geodesic,
                                               double tolerance) {
            return isLocationOnEdgeOrPath(point, polygon, true, geodesic, tolerance);
        }

        /**
         * Same as {@link #isLocationOnEdge(LatLng, List, boolean, double)}
         * with a default tolerance of 0.1 meters.
         */
        public static bool isLocationOnEdge(BasicGeoposition point, List<BasicGeoposition> polygon, bool geodesic) {
            return isLocationOnEdge(point, polygon, geodesic, DEFAULT_TOLERANCE);
        }

        /**
         * Computes whether the given point lies on or near a polyline, within a specified
         * tolerance in meters. The polyline is composed of great circle segments if geodesic
         * is true, and of Rhumb segments otherwise. The polyline is not closed -- the closing
         * segment between the first point and the last point is not included.
         */
        public static bool isLocationOnPath(BasicGeoposition point, List<BasicGeoposition> polyline,
                                               bool geodesic, double tolerance) {
            return isLocationOnEdgeOrPath(point, polyline, false, geodesic, tolerance);
        }

        /**
         * Same as {@link #isLocationOnPath(LatLng, List, boolean, double)}
         *
         * with a default tolerance of 0.1 meters.
         */
        public static bool isLocationOnPath(BasicGeoposition point, List<BasicGeoposition> polyline,
                                               bool geodesic) {
            return isLocationOnPath(point, polyline, geodesic, DEFAULT_TOLERANCE);
        }

        private static bool isLocationOnEdgeOrPath(BasicGeoposition point, List<BasicGeoposition> poly, bool closed,
                                                      bool geodesic, double toleranceEarth) {
            int idx = locationIndexOnEdgeOrPath(point, poly, closed, geodesic, toleranceEarth);

            return (idx >= 0);
        }

        /**
         * Computes whether (and where) a given point lies on or near a polyline, within a specified tolerance.
         * The polyline is not closed -- the closing segment between the first point and the last point is not included.
         * @param point our needle
         * @param poly our haystack
         * @param geodesic the polyline is composed of great circle segments if geodesic
         *                 is true, and of Rhumb segments otherwise
         * @param tolerance tolerance (in meters)
         * @return -1 if point does not lie on or near the polyline.
         *          0 if point is between poly[0] and poly[1] (inclusive),
         *          1 if between poly[1] and poly[2],
         *          ...,
         *          poly.size()-2 if between poly[poly.size() - 2] and poly[poly.size() - 1]
         */
        public static int locationIndexOnPath(BasicGeoposition point, List<BasicGeoposition> poly,
                                               bool geodesic, double tolerance) {
            return locationIndexOnEdgeOrPath(point, poly, false, geodesic, tolerance);
        }

        /**
         * Same as {@link #locationIndexOnPath(LatLng, List, boolean, double)}
         *
         * with a default tolerance of 0.1 meters.
         */
        public static int locationIndexOnPath(BasicGeoposition point, List<BasicGeoposition> polyline,
                                               bool geodesic) {
            return locationIndexOnPath(point, polyline, geodesic, DEFAULT_TOLERANCE);
        }

        /**
         * Computes whether (and where) a given point lies on or near a polyline, within a specified tolerance.
         * If closed, the closing segment between the last and first points of the polyline is not considered.
         * @param point our needle
         * @param poly our haystack
         * @param closed whether the polyline should be considered closed by a segment connecting the last point back to the first one
         * @param geodesic the polyline is composed of great circle segments if geodesic
         *                 is true, and of Rhumb segments otherwise
         * @param toleranceEarth tolerance (in meters)
         * @return -1 if point does not lie on or near the polyline.
         *          0 if point is between poly[0] and poly[1] (inclusive),
         *          1 if between poly[1] and poly[2],
         *          ...,
         *          poly.size()-2 if between poly[poly.size() - 2] and poly[poly.size() - 1]
         */
        private static int locationIndexOnEdgeOrPath(BasicGeoposition point, List<BasicGeoposition> poly, bool closed,
                                              bool geodesic, double toleranceEarth) {
            int size = poly.Count;
            if (size == 0) {
                return -1;
            }
            double tolerance = toleranceEarth / EARTH_RADIUS;
            double havTolerance = hav(tolerance);
            double lat3 = toRadians(point.Latitude);
            double lng3 = toRadians(point.Longitude);
            BasicGeoposition prev = poly[closed ? size - 1 : 0];
            double lat1 = toRadians(prev.Latitude);
            double lng1 = toRadians(prev.Longitude);
            int idx = 0;
            if (geodesic) {
                foreach (BasicGeoposition point2 in poly) {
                    double lat2 = toRadians(point2.Latitude);
                    double lng2 = toRadians(point2.Longitude);
                    if (isOnSegmentGC(lat1, lng1, lat2, lng2, lat3, lng3, havTolerance)) {
                        return Math.Max(0, idx - 1);
                    }
                    lat1 = lat2;
                    lng1 = lng2;
                    idx++;
                }
            } else {
                // We project the points to mercator space, where the Rhumb segment is a straight line,
                // and compute the geodesic distance between point3 and the closest point on the
                // segment. This method is an approximation, because it uses "closest" in mercator
                // space which is not "closest" on the sphere -- but the error is small because
                // "tolerance" is small.
                double minAcceptable = lat3 - tolerance;
                double maxAcceptable = lat3 + tolerance;
                double y1 = mercator(lat1);
                double y3 = mercator(lat3);
                double[] xTry = new double[3];
                foreach (BasicGeoposition point2 in poly) {
                    double lat2 = toRadians(point2.Latitude);
                    double y2 = mercator(lat2);
                    double lng2 = toRadians(point2.Longitude);
                    if (max(lat1, lat2) >= minAcceptable && min(lat1, lat2) <= maxAcceptable) {
                        // We offset longitudes by -lng1; the implicit x1 is 0.
                        double x2 = wrap(lng2 - lng1, -PI, PI);
                        double x3Base = wrap(lng3 - lng1, -PI, PI);
                        xTry[0] = x3Base;
                        // Also explore wrapping of x3Base around the world in both directions.
                        xTry[1] = x3Base + 2 * PI;
                        xTry[2] = x3Base - 2 * PI;
                        foreach (double x3 in xTry) {
                            double dy = y2 - y1;
                            double len2 = x2 * x2 + dy * dy;
                            double t = len2 <= 0 ? 0 : clamp((x3 * x2 + (y3 - y1) * dy) / len2, 0, 1);
                            double xClosest = t * x2;
                            double yClosest = y1 + t * dy;
                            double latClosest = inverseMercator(yClosest);
                            double havDist = havDistance(lat3, latClosest, x3 - xClosest);
                            if (havDist < havTolerance) {
                                return Math.Max(0, idx - 1);
                            }
                        }
                    }
                    lat1 = lat2;
                    lng1 = lng2;
                    y1 = y2;
                    idx++;
                }
            }
            return -1;
        }

        /**
         * Returns sin(initial bearing from (lat1,lng1) to (lat3,lng3) minus initial bearing
         * from (lat1, lng1) to (lat2,lng2)).
         */
        private static double sinDeltaBearing(double lat1, double lng1, double lat2, double lng2,
                                              double lat3, double lng3) {
            double sinLat1 = sin(lat1);
            double cosLat2 = cos(lat2);
            double cosLat3 = cos(lat3);
            double lat31 = lat3 - lat1;
            double lng31 = lng3 - lng1;
            double lat21 = lat2 - lat1;
            double lng21 = lng2 - lng1;
            double a = sin(lng31) * cosLat3;
            double c = sin(lng21) * cosLat2;
            double b = sin(lat31) + 2 * sinLat1 * cosLat3 * hav(lng31);
            double d = sin(lat21) + 2 * sinLat1 * cosLat2 * hav(lng21);
            double denom = (a * a + b * b) * (c * c + d * d);
            return denom <= 0 ? 1 : (a * d - b * c) / sqrt(denom);
        }

        private static bool isOnSegmentGC(double lat1, double lng1, double lat2, double lng2,
                                             double lat3, double lng3, double havTolerance) {
            double havDist13 = havDistance(lat1, lat3, lng1 - lng3);
            if (havDist13 <= havTolerance) {
                return true;
            }
            double havDist23 = havDistance(lat2, lat3, lng2 - lng3);
            if (havDist23 <= havTolerance) {
                return true;
            }
            double sinBearing = sinDeltaBearing(lat1, lng1, lat2, lng2, lat3, lng3);
            double sinDist13 = sinFromHav(havDist13);
            double havCrossTrack = havFromSin(sinDist13 * sinBearing);
            if (havCrossTrack > havTolerance) {
                return false;
            }
            double havDist12 = havDistance(lat1, lat2, lng1 - lng2);
            double term = havDist12 + havCrossTrack * (1 - 2 * havDist12);
            if (havDist13 > term || havDist23 > term) {
                return false;
            }
            if (havDist12 < 0.74) {
                return true;
            }
            double cosCrossTrack = 1 - 2 * havCrossTrack;
            double havAlongTrack13 = (havDist13 - havCrossTrack) / cosCrossTrack;
            double havAlongTrack23 = (havDist23 - havCrossTrack) / cosCrossTrack;
            double sinSumAlongTrack = sinSumFromHav(havAlongTrack13, havAlongTrack23);
            return sinSumAlongTrack > 0;  // Compare with half-circle == PI using sign of sin().
        }

        /**
         * Simplifies the given poly (polyline or polygon) using the Douglas-Peucker decimation
         * algorithm.  Increasing the tolerance will result in fewer points in the simplified polyline
         * or polygon.
         *
         * When the providing a polygon as input, the first and last point of the list MUST have the
         * same latitude and longitude (i.e., the polygon must be closed).  If the input polygon is not
         * closed, the resulting polygon may not be fully simplified.
         *
         * The time complexity of Douglas-Peucker is O(n^2), so take care that you do not call this
         * algorithm too frequently in your code.
         *
         * @param poly polyline or polygon to be simplified.  Polygon should be closed (i.e.,
         *              first and last points should have the same latitude and longitude).
         * @param tolerance in meters.  Increasing the tolerance will result in fewer points in the
         *                  simplified poly.
         * @return a simplified poly produced by the Douglas-Peucker algorithm
         */
        public static List<BasicGeoposition> simplify(List<BasicGeoposition> poly, double tolerance) {
            int n = poly.Count;
            if (n < 1) {
                throw new Exception("Polyline must have at least 1 point");
            }
            if (tolerance <= 0) {
                throw new Exception("Tolerance must be greater than zero");
            }

            bool closedPolygon = isClosedPolygon(poly);
            BasicGeoposition lastPoint = new BasicGeoposition();

            // Check if the provided poly is a closed polygon
            if (closedPolygon) {
                // Add a small offset to the last point for Douglas-Peucker on polygons (see #201)
                double OFFSET = 0.00000000001;
                lastPoint = poly[poly.Count - 1];
                // LatLng.latitude and .longitude are immutable, so replace the last point
                poly.RemoveAt(poly.Count - 1);
                BasicGeoposition position = new BasicGeoposition { Latitude = lastPoint.Latitude + OFFSET, Longitude = lastPoint.Longitude + OFFSET };
                poly.Add(position);
            }

            int idx;
            int maxIdx = 0;
            Stack<int[]> stack = new Stack<int[]>();
            double[] dists = new double[n];
            dists[0] = 1;
            dists[n - 1] = 1;
            double maxDist;
            double dist = 0.0;
            int[] current;

            if (n > 2) {
                int[] stackVal = new int[] { 0, (n - 1) };
                stack.Push(stackVal);
                while (stack.Count > 0) {
                    current = stack.Pop();
                    maxDist = 0;
                    for (idx = current[0] + 1; idx < current[1]; ++idx) {
                        dist = distanceToLine(poly[idx], poly[current[0]],
                                poly[current[1]]);
                        if (dist > maxDist) {
                            maxDist = dist;
                            maxIdx = idx;
                        }
                    }
                    if (maxDist > tolerance) {
                        dists[maxIdx] = maxDist;
                        int[] stackValCurMax = { current[0], maxIdx };
                        stack.Push(stackValCurMax);
                        int[] stackValMaxCur = { maxIdx, current[1] };
                        stack.Push(stackValMaxCur);
                    }
                }
            }

            if (closedPolygon) {
                // Replace last point w/ offset with the original last point to re-close the polygon
                poly.RemoveAt(poly.Count - 1);
                poly.Add(lastPoint);
            }

            // Generate the simplified line
            idx = 0;
            List<BasicGeoposition> simplifiedLine = new List<BasicGeoposition>();
            foreach (BasicGeoposition l in poly) {
                if (dists[idx] != 0) {
                    simplifiedLine.Add(l);
                }
                idx++;
            }

            return simplifiedLine;
        }

        /**
         * Returns true if the provided list of points is a closed polygon (i.e., the first and last
         * points are the same), and false if it is not
         * @param poly polyline or polygon
         * @return true if the provided list of points is a closed polygon (i.e., the first and last
         * points are the same), and false if it is not
         */
        public static bool isClosedPolygon(List<BasicGeoposition> poly) {
            BasicGeoposition firstPoint = poly[0];
            BasicGeoposition lastPoint = poly[poly.Count - 1];

            return (firstPoint.Longitude == lastPoint.Longitude &&
                firstPoint.Latitude == lastPoint.Latitude);

        }

        /**
        * Returns the distance between two LatLngs, in meters.
        */
        public static double computeDistanceBetween(BasicGeoposition from, BasicGeoposition to) {
            return computeAngleBetween(from, to) * EARTH_RADIUS;
        }

        /**
         * Returns the angle between two LatLngs, in radians. This is the same as the distance
         * on the unit sphere.
         */
        static double computeAngleBetween(BasicGeoposition from, BasicGeoposition to) {
            return distanceRadians(toRadians(from.Latitude), toRadians(from.Longitude),
                                   toRadians(to.Latitude), toRadians(to.Longitude));
        }

        /**
         * Returns distance on the unit sphere; the arguments are in radians.
         */
        private static double distanceRadians(double lat1, double lng1, double lat2, double lng2) {
            return arcHav(havDistance(lat1, lat2, lng1 - lng2));
        }


        /**
         * Computes the distance on the sphere between the point p and the line segment start to end.
         *
         * @param p the point to be measured
         * @param start the beginning of the line segment
         * @param end the end of the line segment
         * @return the distance in meters (assuming spherical earth)
         */
        public static double distanceToLine(BasicGeoposition p, BasicGeoposition start, BasicGeoposition end) {

            if (start.Longitude == end.Longitude &&
                start.Latitude == end.Latitude) {
                return computeDistanceBetween(end, p);
            }

            double s0lat = toRadians(p.Latitude);
            double s0lng = toRadians(p.Longitude);
            double s1lat = toRadians(start.Latitude);
            double s1lng = toRadians(start.Longitude);
            double s2lat = toRadians(end.Latitude);
            double s2lng = toRadians(end.Longitude);

            double s2s1lat = s2lat - s1lat;
            double s2s1lng = s2lng - s1lng;
            double u = ((s0lat - s1lat) * s2s1lat + (s0lng - s1lng) * s2s1lng)
                    / (s2s1lat * s2s1lat + s2s1lng * s2s1lng);
            if (u <= 0) {
                return computeDistanceBetween(p, start);
            }
            if (u >= 1) {
                return computeDistanceBetween(p, end);
            }
            BasicGeoposition sa = new BasicGeoposition { Latitude = p.Latitude - start.Latitude, Longitude = p.Longitude - start.Longitude };
            BasicGeoposition sb = new BasicGeoposition { Latitude = u * (end.Latitude - start.Latitude), Longitude = u * (end.Longitude - start.Longitude) };
            return computeDistanceBetween(sa, sb);
        }

        /**
         * Decodes an encoded path string into a sequence of LatLngs.
         */
        public static List<BasicGeoposition> decode(string encodedPath) {
            int len = encodedPath.Length;

            // For speed we preallocate to an upper bound on the final length, then
            // truncate the array before returning.
            List<BasicGeoposition> path = new List<BasicGeoposition>();
            int index = 0;
            int lat = 0;
            int lng = 0;

            while (index < len) {
                int result = 1;
                int shift = 0;
                int b;
                do {
                    b = encodedPath[index++] - 63 - 1;
                    result += b << shift;
                    shift += 5;
                } while (b >= 0x1f);
                lat += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                result = 1;
                shift = 0;
                do {
                    b = encodedPath[index++] - 63 - 1;
                    result += b << shift;
                    shift += 5;
                } while (b >= 0x1f);
                lng += (result & 1) != 0 ? ~(result >> 1) : (result >> 1);

                path.Add(new BasicGeoposition { Latitude = lat * 1e-5, Longitude = lng * 1e-5 });
            }

            return path;
        }

        /**
         * Encodes a sequence of LatLngs into an encoded path string.
         */
        public static String encode(List<BasicGeoposition> path) {
            long lastLat = 0;
            long lastLng = 0;

            StringBuilder result = new StringBuilder();

            foreach (BasicGeoposition point in path) {
                long lat = (long) Math.Round(point.Latitude * 1e5);
                long lng = (long) Math.Round(point.Longitude * 1e5);

                long dLat = lat - lastLat;
                long dLng = lng - lastLng;

                encode(dLat, result);
                encode(dLng, result);

                lastLat = lat;
                lastLng = lng;
            }
            return result.ToString();
        }

        private static void encode(long v, StringBuilder result) {
            v = v < 0 ? ~(v << 1) : v << 1;
            while (v >= 0x20) {
                result.Append(Convert.ToChar((int)((0x20 | (v & 0x1f)) + 63)));
                v >>= 5;
            }
            result.Append(Convert.ToChar((int)(v + 63)));
        }
    }

}
