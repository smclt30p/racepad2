/*
* SettingsManager - Copyright 2017 Supremacy Software
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

using Windows.Storage;


namespace Racepad2.Core.Util {

    /// <summary>
    /// This class is used for persistent storage of key-value pairs 
    /// in string form.
    /// </summary>
    class SettingsManager {

        private static SettingsManager Instance = null;

        /// <summary>
        /// Get the default storage manager for Racepad2
        /// </summary>
        /// <returns></returns>
        public static SettingsManager GetDefaultSettingsManager() {
            if (Instance == null) {
                Instance = new SettingsManager();
            }
            return Instance;
        }

        /// <summary>
        /// Get a setting from the persistent storage. If the key is not found
        /// the <see cref="def"/> value is retured.
        /// </summary>
        /// <param name="key">The key to be looked up</param>
        /// <param name="def">The default value if the key is missing</param>
        /// <returns>The stored value of key if found, else default</returns>
        public string GetSetting(string key, string def) {
            ApplicationDataContainer container = this.GetStorageContainer();
            if (container.Values[key] == null) {
                container.Values[key] = def;
                return def;
            }
            return container.Values[key].ToString();
        }

        /// <summary>
        /// Store a setting into persistent storage
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void PutSetting(string key, string value) {
            ApplicationDataContainer Container = this.GetStorageContainer();
            Container.Values[key] = value;
        }

        /// <summary>
        /// Get the UWP storage container for further usage
        /// </summary>
        /// <returns>the UWP storage container</returns>
        private ApplicationDataContainer GetStorageContainer() {
            Windows.Storage.ApplicationData Appdata = Windows.Storage.ApplicationData.Current;
            Windows.Storage.ApplicationDataContainer Settings = Appdata.LocalSettings;
            Windows.Storage.ApplicationDataContainer Container = null;
            if (!Settings.Containers.ContainsKey("appdata")) {
                Container = Settings.CreateContainer("appdata", Windows.Storage.ApplicationDataCreateDisposition.Always);
            } else {
                Container = Settings.Containers["appdata"];
            }
            return Container;
        }
    }
}