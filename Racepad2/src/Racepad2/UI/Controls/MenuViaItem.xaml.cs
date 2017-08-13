/*
* MenuViaItem - Copyright 2017 Supremacy Software
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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;


namespace Racepad2.UI.Controls {

    /// <summary>
    /// This control represents a item in the via list
    /// </summary>
    public sealed partial class MenuViaItem : UserControl {

        /// <summary>
        /// This event gets fired when the little "X" is pressed
        /// next to a via point
        /// </summary>
        public delegate void ItemRemoved(object sender, ItemRemovedArgs args);
        public event ItemRemoved ItemRemoveRequested;
        private void OnItemRemoveRequested(Waypoint point) {
            if (ItemRemoveRequested == null) return;
            ItemRemovedArgs args = new ItemRemovedArgs() {
                Waypoint = point
            };
            ItemRemoveRequested(this, args);
        }

        /// <summary>
        /// The text property, the actual text in the control
        /// </summary>
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

        /// <summary>
        /// The type of via, this sets the icon
        /// </summary>
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
        
        /// <summary>
        /// This is the model behind the control, used
        /// for event handling
        /// </summary>
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

        /// <summary>
        /// This is the internal event fired, its 
        /// a fire-through, it fires the OnItemRemoveRequested
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            OnItemRemoveRequested(Waypoint);
        }

    }

    public class ItemRemovedArgs : EventArgs {
        public Waypoint Waypoint { get; set; }
    }

}
