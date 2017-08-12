/*
* MainPage - Copyright 2017 Supremacy Software
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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Racepad2 {

    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
        }

        /// <summary>
        /// The back button has been pressed
        /// </summary>        
        private void MainPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e) {
            Frame frame = (Frame)Window.Current.Content;
            if (frame.CanGoBack && e.Handled == false) {
                frame.GoBack();
                e.Handled = true;
            }
        }

        /// <summary>
        /// The RIDE button is pressed
        /// </summary>
        private void Ride_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(NavigationPage), null);
        }

        /// <summary>
        /// The HISTORY button is pressed
        /// </summary>
        private void History_Click(object sender, RoutedEventArgs e) {
        }

        /// <summary>
        /// The SETTINGS button is pressed
        /// </summary>
        private void Settings_Click(object sender, RoutedEventArgs e) {
        }

        /// <summary>
        /// The COURSES button is pressed
        /// </summary>
        private void Courses_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(RouteBrowser));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            Frame.BackStack.Clear();
        }

        /// <summary>
        /// The VIEW MAP button is pressed
        /// </summary>
        private void ViewMap_Click(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(MapViewPage));
        }
    }
}
