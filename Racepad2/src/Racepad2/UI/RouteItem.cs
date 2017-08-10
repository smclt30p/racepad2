using System;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Racepad2.UI {
    class RouteItem : UserControl {

        public IStorageItem File { get; set; }

        public string UpperText {
            get {
                return Title.Text;
            } set {
                Title.Text = value;
            }
        }

        public string LowerText {
            get {
                return Path.Text;
            } set {
                Path.Text = value;
            }
        }

        public RouteItem() {
            InitializeComponent();
        }

        public event RoutedEventHandler Click;

        private Grid Grid { get; set; }
        private Image Image { get; set; }
        private TextBlock Title { get; set; }
        private TextBlock Path { get; set; }
        private Button Button { get; set; }

        private void InitializeComponent() {

            Grid = new Grid();

            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();

            GridLength col1width = new GridLength(1, GridUnitType.Auto);

            ColumnDefinition col1 = new ColumnDefinition() {
                Width = col1width
            };
            ColumnDefinition col2 = new ColumnDefinition();

            Grid.RowDefinitions.Add(row1);
            Grid.RowDefinitions.Add(row2);
            Grid.ColumnDefinitions.Add(col1);
            Grid.ColumnDefinitions.Add(col2);

            Image = new Image() {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Icons/icon-routes.png")),
                Margin = new Windows.UI.Xaml.Thickness(5),
                Height = 40,
                Width = 40
            };

            Grid.SetRowSpan(Image, 2);
            Grid.SetRow(Image, 0);
            Grid.SetColumn(Image, 0);
            Grid.Children.Add(Image);

            Title = new TextBlock() {
                Margin = new Windows.UI.Xaml.Thickness(5),
                Text = "Test text"
            };

            Grid.SetColumn(Title, 1);
            Grid.SetRow(Title, 0);
            Grid.Children.Add(Title);


            Path = new TextBlock() {
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Gray),
                Margin = new Windows.UI.Xaml.Thickness(5, 0, 0, 0)
            };

            Grid.SetRow(Path, 1);
            Grid.SetColumn(Path, 1);
            Grid.Children.Add(Path);

            Button = new Button() {
                Content = Grid,
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            Button.Click += Button_Click;

            base.Content = Button;

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Click(this, e);
        }
    }
}

