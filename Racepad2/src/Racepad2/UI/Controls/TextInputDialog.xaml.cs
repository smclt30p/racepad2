using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace Racepad2.UI.Controls {
    public sealed partial class TextInputDialog : ContentDialog {

        public readonly DependencyProperty TextProperty = DependencyProperty.Register("InnerText", typeof(string), typeof(TextInputDialog), null);
        public string InnerText {
            get {
                return (string) GetValue(TextProperty);
            }
            set {
                SetValue(TextProperty, value);
            }
        }

        public TextInputDialog() {
            this.InitializeComponent();
        }

        public TextInputDialog(string v) {
            base.Title = v;
            this.InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            base.Hide();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) {
            base.Hide();
        }
    }
}
