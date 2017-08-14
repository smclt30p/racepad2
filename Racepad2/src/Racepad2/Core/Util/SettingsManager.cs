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

using System.Collections.Generic;
using System.IO;
using System;
using System.Xml.Serialization;
using System.Text;

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

        private List<Setting> SettingsList { get; set; }

        /// <summary>
        /// Get a setting from the persistent storage. If the key is not found
        /// the <see cref="def"/> value is retured.
        /// </summary>
        /// <param name="key">The key to be looked up</param>
        /// <param name="def">The default value if the key is missing</param>
        /// <returns>The stored value of key if found, else default</returns>
        public string GetSetting(string key, string def) {
            foreach (Setting setting in SettingsList) {
                if (setting.Key == key) return setting.Value;
            }
            return def;
        }

        /// <summary>
        /// Store a setting into persistent storage
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        public void PutSetting(string key, string value) {
            foreach (Setting setting in SettingsList) {
                if (setting.Key == key) {
                    setting.Value = value;
                    return;
                }
            }
            SettingsList.Add(new Setting { Key = key, Value = value });
        }

        /// <summary>
        /// Reads a list from persistent storage
        /// </summary>
        public List<T> ReadList<T>(string key) {
            string data = GetSetting(key, "null");
            if (data == "null") return null;
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            StringReader reader = new StringReader(data);
            return (List<T>) serializer.Deserialize(reader);
        }

        /// <summary>
        /// Writes a list to persistent storage
        /// </summary>
        public void WriteList<T>(string key, List<T> theList) {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, theList);
            PutSetting(key, writer.ToString());
        }

        /// <summary>
        /// Writes all the settings to the settings.xml file
        /// </summary>
        public void CommitToStorage() {
            StringWriter writer = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(typeof(List<Setting>));
            serializer.Serialize(writer, SettingsList);
            File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "/settings.xml", writer.ToString(), Encoding.Unicode);
        }

        /// <summary>
        /// Reads the settings.xml into memory.
        /// If the settings.xml file does not exist a new, blank one
        /// is created and populated with an empty XML structure.
        /// </summary>
        public void ReadFromStorage() {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Setting>));
            try {
                string data = File.ReadAllText(ApplicationData.Current.LocalFolder.Path + "/settings.xml", Encoding.Unicode);
                StringReader reader = new StringReader(data);
                SettingsList = (List<Setting>)serializer.Deserialize(reader);
            } catch (Exception) {
                StringWriter writer = new StringWriter();
                serializer.Serialize(writer, SettingsList);
                File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "/settings.xml", writer.ToString(), Encoding.Unicode);
                SettingsList = new List<Setting>();
            }
        }
    }

    /// <summary>
    /// Represents a simple key-value pair
    /// </summary>
    public class Setting {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}