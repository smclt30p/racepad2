/*
* FileReader - Copyright 2017 Supremacy Software
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
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

using Racepad2.Core.Util;


namespace Racepad2.Core {

    class FileReader {

        /// <summary>
        /// Reads a file as a string
        /// </summary>
        /// <param name="item">the StorageItem item to be read</param>
        /// <returns>the contents of the storageitem</returns>
        public async static Task<string> ReadFile(IStorageItem item) {
            if (item.IsOfType(StorageItemTypes.File)) {
                StorageFile file = item as StorageFile;
                return await FileIO.ReadTextAsync(file);
            }
            return null;
        }

        /// <summary>
        /// Load the storage folder from the phone's memory
        /// where the courses are placed in GPX format.
        /// </summary>
        /// <returns></returns>
        public async static Task<StorageFolder> ReadFolder(String key) {
            string routeToken = SettingsManager.GetDefaultSettingsManager().GetSetting(key, "null");
            if (routeToken == "null") {
                FolderPicker picker = new FolderPicker();
                picker.FileTypeFilter.Add("*");
                picker.CommitButtonText = "Select folder";
                StorageFolder folder = await picker.PickSingleFolderAsync();
                string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
                SettingsManager.GetDefaultSettingsManager().PutSetting(key, token);
                return folder;
            } else {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(routeToken)) {
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(routeToken);
                } else {
                    throw new Exception("Folder inaccesible!");
                }
            }
        }

    }
}
