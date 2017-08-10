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

using Racepad2.Core.Util;
using Racepad2.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Racepad2 { 

    public sealed partial class RouteBrowser : Page {

        private SettingsManager SettingsManager = SettingsManager.GetDefaultSettingsManager();

        public RouteBrowser() {
            this.InitializeComponent();
            Load();
        }

        /// <summary>
        /// The main async method to load files into the UI
        /// </summary>
        private void Load() {
            Task.Run(async () => {
                ClearList();
                SetShowProgress(Visibility.Visible);
                StorageFolder folder = await LoadRouteFolder();
                IReadOnlyList<IStorageItem> items = await LoadRoutesFromFolder(folder);
                await AddItemsToList(items);
                SetShowProgress(Visibility.Collapsed);
            });
        }

        /// <summary>
        /// Load the storage folder from the phone's memory
        /// where the courses are placed in GPX format.
        /// </summary>
        /// <returns></returns>
        private async Task<StorageFolder> LoadRouteFolder() {
            string routeToken = SettingsManager.GetSetting("RoutePath", "null");
            if (routeToken == "null") {
                FolderPicker picker = new FolderPicker();
                picker.FileTypeFilter.Add("*");
                picker.CommitButtonText = "Select route folder";
                StorageFolder folder = await picker.PickSingleFolderAsync();
                string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
                SettingsManager.PutSetting("RoutePath", token);
                return folder;
            } else {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(routeToken)) {
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(routeToken);
                } else {
                    throw new Exception("Routes folder inaccesible!");
                }
            }
        }

        /// <summary>
        /// Loads the routes from the folder and returns the routes
        /// as a list of <see cref="IStorageItem"/>s
        /// </summary>
        private async Task<IReadOnlyList<IStorageItem>> LoadRoutesFromFolder(StorageFolder folder) {
            StorageItemQueryResult result = folder.CreateItemQuery();
            return await result.GetItemsAsync();
        }

        /// <summary>
        /// Add's a storage item that represents a GPX file to the list of
        /// files in the UI
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private async Task<object> AddItemsToList(IReadOnlyList<IStorageItem> items) {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (items.Count == 0) {
                    List.Children.Remove(Progress);
                    TextBlock msg = new TextBlock() {
                        FontSize = 10,
                        Text = "No routes found",
                        TextAlignment = TextAlignment.Center
                    };
                    List.Children.Add(msg);
                    return;
                }
                foreach (IStorageItem item in items) {
                    List.Children.Remove(Progress);
                    RouteItem routeItem = new RouteItem();
                    routeItem.UpperText = item.Name;
                    routeItem.LowerText = item.Path;
                    routeItem.File = item;
                    routeItem.Click += Item_Click;
                    List.Children.Add(routeItem);
                }
            });
            return null;
        }

        /// <summary>
        /// Clears the list of items displayed
        /// TODO: Remove this item and abstract away the UI operations with binding
        /// </summary>
        private async void ClearList() {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                List.Children.Clear();
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
        /// Occurs when a item in the list is clicked
        /// </summary>
        private void Item_Click(object sender, RoutedEventArgs e) {
            RouteItem item = sender as RouteItem;
            Frame.Navigate(typeof(RoutePreview), item.File);
        }

        /// <summary>
        /// Occurs when the reload button is pressed
        /// </summary>
        private void Reload_Click(object sender, RoutedEventArgs e) {
            Load();
        }
    }

    /// <summary>
    /// This class represents a user-selectable item in the route browser, that
    /// includes a icon, title and description.
    /// </summary>
    class RouteItem : UserControl {

        public IStorageItem File { get; set; }
        
        /// <summary>
        /// The upper text, the title
        /// </summary>
        public string UpperText {
            get {
                return Title.Text;
            }
            set {
                Title.Text = value;
            }
        }

        /// <summary>
        /// The lower text, the description
        /// </summary>
        public string LowerText {
            get {
                return Path.Text;
            }
            set {
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

        /// <summary>
        /// Initialize the view hierarchy
        /// </summary>
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

        /// <summary>
        /// This method is a event passthrough from the button click
        /// that wraps the control. This is fired when the item is
        /// clicked in the list.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e) {
            Click(this, e);
        }

    }

}
