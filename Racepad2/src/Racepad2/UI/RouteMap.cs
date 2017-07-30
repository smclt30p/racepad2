using Racepad2.Geo.Navigation.Core;
using System;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

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
                RotateInteractionMode = MapInteractionMode.PointerKeyboardAndControl
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
                Margin = new Windows.UI.Xaml.Thickness(10)
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

        /** Display a Route object on the map. 
         */
        public void DisplayRouteOnMap(Route route) {

            Geopath path = new Geopath(route.Path);
            MapPolyline pol = new MapPolyline();

            Geopoint start = new Geopoint(route.Path[0]);

            pol.StrokeThickness = 5;
            pol.StrokeColor = Colors.MediumPurple;
            pol.Path = path;

            UserLocation = new MapIcon() {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/arrow.png"))
            };

            Map.MapElements.Add(UserLocation);
            Map.MapElements.Add(pol);

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

        public async void PreviewRoute(Route route) {

            GeoboundingBox box = GeoboundingBox.TryCompute(route.Path);
            await Map.TrySetViewBoundsAsync(box, new Windows.UI.Xaml.Thickness(20), MapAnimationKind.None);

        }

    }
}
