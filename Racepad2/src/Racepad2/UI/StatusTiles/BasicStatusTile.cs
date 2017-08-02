using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Racepad2.UI.StatusTiles {

    abstract class BasicStatusTile : UserControl {

        private Grid Host { get; set; }

        private TextBlock _value;
        private TextBlock _description;

        public virtual object Value {
            get {
                return _value.Text;
            } set {
                _value.Text = value.ToString();
            }
        }

        public string BottomText {
            get {
                return _description.Text;
            } set {
                _description.Text = value;
            }
        }

        public BasicStatusTile() {
            InitializeComponent();
        }

        private void InitializeComponent() {

            Host = new Grid();

            RowDefinition top = new RowDefinition();
            RowDefinition bottom = new RowDefinition();
            
            GridLength topGrid = new GridLength(1, GridUnitType.Auto);
            GridLength bottomGrid = new GridLength(1, GridUnitType.Star);

            top.Height = topGrid;
            bottom.Height = bottomGrid;

            Host.RowDefinitions.Add(top);
            Host.RowDefinitions.Add(bottom);

            _description = new TextBlock() {
                Text = "Undefined",
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top,
                TextAlignment = Windows.UI.Xaml.TextAlignment.Center,
                Margin = new Thickness(5)
            };

            FontWeight weight = new FontWeight();
            weight.Weight = 600;

            _value = new TextBlock() {
                Text = "NaN",
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = Windows.UI.Xaml.TextAlignment.Center,
                FontSize = 48,
                Margin = new Thickness(0, 20, 0, 0),
                FontWeight = weight
            };

            Host.Children.Insert(0, _description);
            Host.Children.Insert(1, _value);

            base.Content = Host;

            base.SizeChanged += BasicStatusTile_SizeChanged;

        }

        private void BasicStatusTile_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e) {

            double third = Host.ActualHeight / 6;
            double width = Host.ActualWidth;

            _description.FontSize = third;

            if (_value.ActualWidth > width) {
                _value.FontSize = third * 2;
            } else {
                _value.FontSize = third * 3;
            }

        }

    }

}
