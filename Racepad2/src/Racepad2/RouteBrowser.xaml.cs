/*
* RouteBrowser - Copyright 2017 Supremacy Software
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
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Racepad2.Core;
using Racepad2.Core.Util;


namespace Racepad2 {

    public sealed partial class RouteBrowser : Page {

        private SettingsManager SettingsManager = SettingsManager.GetDefaultSettingsManager();
        private List<RouteItem> DisplayedItems { get; set; }

        public RouteBrowser() {
            DisplayedItems = new List<RouteItem>();
            this.InitializeComponent();
            Load();
        }

        /// <summary>
        /// The main async method to load files into the UI
        /// </summary>
        private async void Load() {
            StorageFolder folder = await FileReader.ReadFolder("RoutePath");
            SetShowProgress(Visibility.Visible);
            StorageItemQueryResult result = folder.CreateItemQuery();
            IReadOnlyList<IStorageItem> items = await result.GetItemsAsync();
            AddItemsToList(items);
            SetShowProgress(Visibility.Collapsed);
        }

        /// <summary>
        /// Add's a storage item that represents a GPX file to the list of
        /// files in the UI
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private async void AddItemsToList(IReadOnlyList<IStorageItem> items) {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                /* There are no items to display */
                if (items.Count == 0) {
                    TextBlock msg = new TextBlock() {
                        FontSize = 10,
                        Text = "No routes found",
                        TextAlignment = TextAlignment.Center
                    };
                    return;
                }
                foreach (IStorageItem item in items) {
                    RouteItem routeItem = new RouteItem();
                    routeItem.UpperText = item.Name;
                    routeItem.LowerText = item.Path;
                    routeItem.File = item;
                    DisplayedItems.Add(routeItem);
                }
                ItemList.ItemsSource = DisplayedItems;
            });
        }

        /// <summary>
        /// Display the progress meter
        /// </summary>
        /// <param name="status"></param>
        private async void SetShowProgress(Visibility status) {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Progress.Visibility = status;
            });
        }

        /// <summary>
        /// Occurs when the reload button is pressed
        /// </summary>
        private void Reload_Click(object sender, RoutedEventArgs e) {
            Load();
        }

        /// <summary>
        /// This event is fired when a route is clicked
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            RouteItem item = ((Button)sender).Tag as RouteItem;
            Frame.Navigate(typeof(RoutePreview), item.File);
        }
    }

    /// <summary>
    /// This class represents a user-selectable item in the route browser, that
    /// includes a icon, title and description.
    /// </summary>
    class RouteItem {
        public IStorageItem File { get; set; }
        public string UpperText { get; set; }
        public string LowerText { get; set; }
    }

}
