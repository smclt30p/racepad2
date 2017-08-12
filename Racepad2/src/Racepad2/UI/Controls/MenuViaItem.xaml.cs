using System;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Racepad2.UI.Controls {

    public sealed partial class MenuViaItem : UserControl {

        public delegate void ItemRemoved(object sender, ItemRemovedArgs args);
        public event ItemRemoved ItemRemoveRequested;
        private void OnItemRemoveRequested(Waypoint point) {
            if (ItemRemoveRequested == null) return;
            ItemRemovedArgs args = new ItemRemovedArgs();
            args.Waypoint = point;
            ItemRemoveRequested(this, args);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MenuViaItem), new PropertyMetadata("TEST", TextPropertyChanged));
        private static void TextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MenuViaItem inst = d as MenuViaItem;
            inst.DescText.Text = (string)e.NewValue;
        }
        public string Text {
            get {
                return (string) GetValue(TextProperty);
            }
            set {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty ViaTypeProperty = DependencyProperty.Register("ViaType", typeof(ViaType), typeof(MenuViaItem), new PropertyMetadata(null, ViaChanged));
        private static void ViaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            MenuViaItem item = d as MenuViaItem;
            switch ((ViaType)e.NewValue) {
                case ViaType.End:
                    item.StartImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icons/B.png"));
                    break;
                case ViaType.Start:
                    item.StartImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icons/A.png"));
                    break;
                case ViaType.Pivot:
                    item.StartImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/Icons/V.png"));
                    break;
            }
        }
        public ViaType ViaType {
            get {
                return (ViaType)GetValue(ViaTypeProperty);
            }
            set {
                SetValue(ViaTypeProperty, value);
            }
        }

        public static readonly DependencyProperty WaypointProperty = DependencyProperty.Register("Waypoint", typeof(Waypoint), typeof(MenuViaItem), null);
        public Waypoint Waypoint {
            get {
                return (Waypoint) GetValue(WaypointProperty);
            } set {
                SetValue(WaypointProperty, value);
            }
        }

        public MenuViaItem() {
            this.InitializeComponent();
        }



        private void Button_Click(object sender, RoutedEventArgs e) {
            OnItemRemoveRequested(Waypoint);
        }

    }

    public class ItemRemovedArgs : EventArgs {
        public Waypoint Waypoint { get; set; }
    }

}
