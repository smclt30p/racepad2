using Racepad2.Geo.Navigation.Core;
using Racepad2.Route;
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

        public RouteMap() : base() {
            InitializeComponent();
        }
           
        public VehiclePosition Position {
            get {
                return _position;
            }
            set {
                if (_userPathData == null) _userPathData = new List<BasicGeoposition>();
                _userPathData.Add(value.Position.Position);
                UpdatePosition(value);
                _position = value;
            }
        }

        public DriveRoute Route {
            get {
                return _route;
            }
            set {
                _route = value;
                SetRoute(value);
            }
        }

        public CoreDispatcher ViewDispatcher { get; set; }

        private DriveRoute _route;
        private VehiclePosition _position;
        private List<BasicGeoposition> _userPathData;
        private string AttachIcon = null;
        private string DetachIcon = null;
        private string PathPreviewIcon = null;
        private MapMode _mode;

        private MapControl Map { get; set; }
        private StackPanel ButtonContainer { get; set; }
        private Button DetachButton { get; set; }
        private Button PathPreviewButton { get; set; }
        private MapIcon UserLocation { get; set; }
        private DriveRoute RouteBackup { get; set; }
        private bool PathPreview { get; set; } = false;

        private MapMode Mode {

            get {
                return _mode;
            }
            set {

                switch (value) {

                    case MapMode.MAP_ATTACHED:
                        Map.ZoomInteractionMode = MapInteractionMode.Disabled;
                        Map.PanInteractionMode = MapPanInteractionMode.Disabled;
                        Map.TiltInteractionMode = MapInteractionMode.Disabled;
                        Map.RotateInteractionMode = MapInteractionMode.Disabled;
                        DetachButton.Content = DetachIcon;
                        break;
                    case MapMode.MAP_DETACHED:
                        Map.ZoomInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                        Map.PanInteractionMode = MapPanInteractionMode.Auto;
                        Map.TiltInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                        Map.RotateInteractionMode = MapInteractionMode.PointerKeyboardAndControl;
                        DetachButton.Content = AttachIcon;
                        Map.Heading = 0;
                        break;

                }

                _mode = value;

            }

        }

        private enum MapMode {
            MAP_ATTACHED, MAP_DETACHED
        }

        private void RemoveAllMapElements() {
            Map.MapElements.Clear();
        }

        

        /** Initialize the view hierarchy and the component
         * children elements.
         */
        private void InitializeComponent() {

            /* Initialize icons */

            DetachIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE17C", 16)).ToString();
            AttachIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE18D", 16)).ToString();
            PathPreviewIcon = System.Convert.ToChar(System.Convert.ToUInt32("0xE81B", 16)).ToString();

            /* Initialize the grid */

            RowDefinition row1 = new RowDefinition();
            base.RowDefinitions.Add(row1);

            /* Initialize the map */

            Map = new MapControl() {
                MapServiceToken = MapUtil.MAP_SERVICE_TOKEN,
                ZoomInteractionMode = MapInteractionMode.Disabled,
                PanInteractionMode = MapPanInteractionMode.Disabled,
                TiltInteractionMode = MapInteractionMode.Disabled,
                RotateInteractionMode = MapInteractionMode.Disabled,
                ColorScheme = MapColorScheme.Dark
            };

            /* Initialize the buttons */

            ButtonContainer = new StackPanel() {
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right,
                Margin = new Windows.UI.Xaml.Thickness(10),
                Orientation = Orientation.Horizontal
            };

            DetachButton = new Button() {
                FontFamily = new Windows.UI.Xaml.Media.FontFamily("Segoe MDL2 Assets"),
                FontSize = 20,
                Content = DetachIcon,
                Height = 40,
                Width = 40,
                Background = new SolidColorBrush(Colors.DarkGray)
            };

            PathPreviewButton = new Button() {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                FontSize = 20,
                Content = PathPreviewIcon,
                Height = 40,
                Width = 40,
                Margin = new Windows.UI.Xaml.Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Colors.DarkGray)
            };

            UserLocation = new MapIcon() {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/arrow.png"))
            };

            Map.MapElements.Add(UserLocation);

            ButtonContainer.Children.Insert(0, PathPreviewButton);
            ButtonContainer.Children.Insert(1, DetachButton);

            DetachButton.Click += DetachButton_Click;
            PathPreviewButton.Click += PathPreviewButton_Click;

            /* Create view tree */

            base.Children.Add(Map);
            base.Children.Add(ButtonContainer);

            Mode = MapMode.MAP_ATTACHED;

        }

        private async void PathPreviewButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {

            if (!PathPreview) {

                if (_userPathData.Count < 2) {
                    return;
                }

                PathPreview = true;
                Mode = MapMode.MAP_DETACHED;
                Map.Heading = 0;

                RouteBackup = Route;
                RemoveAllMapElements();

                Geopath path = new Geopath(_userPathData);
                MapPolyline line = new MapPolyline() {
                    StrokeColor = Colors.Purple,
                    StrokeThickness = 5,
                    Path = path
                };

                GeoboundingBox box = GeoboundingBox.TryCompute(_userPathData);

                Map.MapElements.Add(line);
                await Map.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(20), MapAnimationKind.None);
                DetachButton.IsEnabled = false;

            } else {

                PathPreview = false;
                DetachButton.IsEnabled = true;
                Mode = MapMode.MAP_ATTACHED;
                RemoveAllMapElements();
                Map.MapElements.Add(UserLocation);
                if (RouteBackup != null) {
                    Route = RouteBackup;
                }
            }

        }

        /** Event occurs when the attach/detach button is pressed
         */
        private void DetachButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            if (Mode == MapMode.MAP_DETACHED) {
                Mode = MapMode.MAP_ATTACHED;
            } else {
                Mode = MapMode.MAP_DETACHED;
            }
        }

        


        /* If attached, centers the map onto the users location and 
         * turns the map in the heading direction
         */
        private async void UpdatePosition(VehiclePosition position) {

            await ViewDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {

                if (Mode == MapMode.MAP_ATTACHED) {
                    if (position.Bearing != null) {
                        await Map.TrySetViewAsync(position.Position, 15, (double)position.Bearing, 0, MapAnimationKind.None);
                    }
                }

                UserLocation.Location = position.Position;

            });

        }

        private void SetRoute(DriveRoute value) {

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

                Map.MapElements.Add(polyline);

            }

        }

    }

    public class VehiclePosition {
        public VehiclePosition() {
        }

        public VehiclePosition(Geopoint position, double? bearing) {
            Position = position;
            Bearing = bearing;
        }

        public Geopoint Position { get; set; }
        public double? Bearing { get; set; }
    }


}
