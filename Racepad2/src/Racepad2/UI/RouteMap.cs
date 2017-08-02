using Racepad2.Geo;
using Racepad2.Geo.Navigation.Core;
using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

namespace Racepad2.UI {
    class RouteMap : Grid {

        /* Microsoft's UWP MapControl needs a service token
         * for god-knows what. 
         */
        private const string MAP_SERVICE_TOKEN = 
            "4qiurkdgKlTD5Ba1qkOu~7va9tRX0jj2kZEnXqyt8Iw~AmfqIUMnMk6zor_" +
            "i4yKPKpXJeT3J0FxOsy8Z6BcGKRJ6yZwKRfZsj-ak87UiVnzT";

        /* The actual elements of the attach/detach button and the
         * map.
         */
        private MapControl Map { get; set; }
        private Button DetachButton { get; set; }

        /* The icon of the user location on the map, 
         * a small red arrow pointing to north.
         */
        private MapIcon UserLocation { get; set; }

        /* Indicates if the view is attached to the
         * user or its in free pan mode
         */
        private bool Attached { get; set; }

        /* The UI dispatcher element 
         */
        public CoreDispatcher ViewDispatcher { get; set; }

        private DriveRoute _route;

        public DriveRoute Route {
            get {
                return _route;
            }
            set {

                List<BasicGeoposition> elevationPair = new List<BasicGeoposition>();
                Geopath path;
                MapPolyline polyline;
                BasicGeoposition geo1;
                BasicGeoposition geo2;

                for (int i = 0; ; i++) {

                    if (i + 1 >= value.Path.Count) break;

                    geo1 = value.Path[i];
                    geo2 = value.Path[i + 1];

                    double run = GeoMath.Distance(geo1, geo2);
                    double rise = geo2.Altitude - geo1.Altitude;
                    double percentage = rise / run * 100;

                    /* Null out altitude to lay 
                     * polyline down flat on map */
                    geo1.Altitude = 0;
                    geo2.Altitude = 0;

                    elevationPair.Add(geo1);
                    elevationPair.Add(geo2);

                    path = new Geopath(elevationPair);
                    polyline = new MapPolyline() {
                        StrokeThickness = 5,
                        Path = path,
                        StrokeColor = GetColorFromSlope(percentage)
                    };

                    Map.MapElements.Add(polyline);

                }

                UserLocation = new MapIcon() {
                    Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/arrow.png"))
                };

                Map.MapElements.Add(UserLocation);

                _route = value;
            }
        }

        private Color GetColorFromSlope(double percentage) {

            if (percentage < -20) return GetSolidColorBrush("00F6FF");
            if (percentage > 20) return GetSolidColorBrush("FF0000");

            if (percentage >= -20 && percentage <= -15) return GetSolidColorBrush("00FFB0");
            if (percentage >= -15 && percentage <= -10) return GetSolidColorBrush("00FF77");
            if (percentage >= -10 && percentage <= -5) return GetSolidColorBrush("00FF4C");
            if (percentage >= -5 && percentage <= 0) return GetSolidColorBrush("00FF00");
            if (percentage >= 0 && percentage <= 5) return GetSolidColorBrush("00FF00");
            if (percentage >= 5 && percentage <= 10) return GetSolidColorBrush("FFFF00");
            if (percentage >= 10 && percentage <= 15) return GetSolidColorBrush("FFBD00");
            if (percentage >= 15 && percentage <= 20) return GetSolidColorBrush("FF8200");

            return Colors.Purple;
        }

        /* Thanks to Joel Joseph for this method! */
        public Color GetSolidColorBrush(string hex) {
            hex = hex.Replace("#", string.Empty);
            byte a = 255;
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush.Color;
        }

        private string AttachIcon = null;
        private string DetachIcon = null;

        public RouteMap() : base() {
            InitializeComponent();
        }

        /** Initialize the view hierarchy and the component
         * children elements.
         */
        private void InitializeComponent() {

            /* Initialize icons */

            DetachIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE17C", 16)).ToString();
            AttachIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE18D", 16)).ToString();

            /* Misc init */

            Attached = false;

            /* Initialize the grid */

            RowDefinition row1 = new RowDefinition();
            base.RowDefinitions.Add(row1);

            /* Initialize the map */

            Map = new MapControl() {
                MapServiceToken = MAP_SERVICE_TOKEN,
                ZoomInteractionMode = MapInteractionMode.PointerKeyboardAndControl,
                PanInteractionMode = MapPanInteractionMode.Auto,
                TiltInteractionMode = MapInteractionMode.PointerKeyboardAndControl,
                RotateInteractionMode = MapInteractionMode.PointerKeyboardAndControl,
                ColorScheme = MapColorScheme.Dark
            };

            /* Initialize the button */

            DetachButton = new Button() {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 20,
                Content = AttachIcon,
                Height = 40,
                Width = 40,
                Margin = new Windows.UI.Xaml.Thickness(10),
                Background = new SolidColorBrush(Colors.DarkGray)
            };

            DetachButton.Click += DetachButton_Click;

            /* Create view tree */

            base.Children.Add(Map);
            base.Children.Add(DetachButton);

        }

        /** Event occurs when the attach/detach button is pressed
         */
        private void DetachButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {

            if (Attached) {
                Map.ZoomInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                Map.PanInteractionMode = MapPanInteractionMode.Auto;
                Map.TiltInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                Map.RotateInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                Attached = false;
                DetachButton.Content = AttachIcon;
            } else {
                Map.ZoomInteractionMode = MapInteractionMode.Disabled;
                Map.PanInteractionMode = MapPanInteractionMode.Disabled;
                Map.TiltInteractionMode = MapInteractionMode.Disabled;
                Map.RotateInteractionMode = MapInteractionMode.Disabled;
                Attached = true;
                DetachButton.Content = DetachIcon;
            }

        }
    
        

        /* If attached, centers the map onto the users location and 
         * turns the map in the heading direction
         */
        public async void UpdateMapPosition(Geopoint position, double? bearing) {

            await ViewDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                if (Attached) {
                    await Map.TrySetViewAsync(position, 15, (double)bearing, 0, MapAnimationKind.None);
                }

                UserLocation.Location = position;

            });
            
        }

        public async void PreviewRoute(DriveRoute route) {

            GeoboundingBox box = GeoboundingBox.TryCompute(route.Path);
            await Map.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(20), MapAnimationKind.None);

        }

    }
}
