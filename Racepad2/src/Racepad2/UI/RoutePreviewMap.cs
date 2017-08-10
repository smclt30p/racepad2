using Racepad2.Geo.Navigation.Core;
using Racepad2.Route;
using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace Racepad2.UI {

    class RoutePreviewMap : Grid {

        public DriveRoute Route {
            get {

                return _route;

            } set {
                SetRoute(value);
                _route = value;
            }
        }

        public RoutePreviewMap() {
            InitializeComponent();
        }

        private MapControl _innerMap;
        private DriveRoute _route;

        private void InitializeComponent() {

            _innerMap = new MapControl() {
                MapServiceToken = MapUtil.MAP_SERVICE_TOKEN,
                ColorScheme = MapColorScheme.Dark
            };

            base.Children.Add(_innerMap);

        }

        private async void SetRoute(DriveRoute value) {

            List<GradientPair> gradiends = DriveRoute.GetGradientPairsFromRoute(value.Path);

            MapPolyline polyline;
            Geopath path;

            foreach (GradientPair pair in gradiends) {

                path = new Geopath(pair.Path);
                polyline = new MapPolyline() {
                    StrokeThickness = 5,
                    Path = path,
                    StrokeColor = MapUtil.GetColorFromSlope(pair.SlopePercentage)
                };

                _innerMap.MapElements.Add(polyline);

            }

            GeoboundingBox box = GeoboundingBox.TryCompute(value.Path);
            await _innerMap.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(30), MapAnimationKind.None);

        }

    }

}
