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
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Racepad2 {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RouteBrowser : Page {

        private SettingsManager SettingsManager = SettingsManager.GetDefaultSettingsManager();

        public RouteBrowser() {
            this.InitializeComponent();
            Load();
        }

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

        private async Task<IReadOnlyList<IStorageItem>> LoadRoutesFromFolder(StorageFolder folder) {
            StorageItemQueryResult result = folder.CreateItemQuery();
            return await result.GetItemsAsync();
        }

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

        private async void ClearList() {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                List.Children.Clear();
            });
        }

        private async void SetShowProgress(Visibility status) {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Progress.Visibility = status;
            });
        }

        private void Item_Click(object sender, RoutedEventArgs e) {
            RouteItem item = sender as RouteItem;
            Frame.Navigate(typeof(RoutePreview), item.File);
        }

        private void Reload_Click(object sender, RoutedEventArgs e) {
            Load();
        }
    }
}
