/*
* SettingsPage - Copyright 2017 Supremacy Software
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

using Racepad2.Core.Util;
using Racepad2.Core.Util.Conversions;

namespace Racepad2 {

    public sealed partial class SettingsPage : Page {

        public SettingsPage() {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings() {
            // Units switch
            UnitToggleSwitch.IsOn = SettingsManager.GetDefaultSettingsManager().GetSetting("Units", "Metric") == "Imperial";
            TimeFormatToggle.IsOn = SettingsManager.GetDefaultSettingsManager().GetSetting("TimeFormat", "H24") == "H12";
        }

        /// <summary>
        /// This occurs when the unit type is changed
        /// </summary>
        private void UnitToggleSwitch_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            ToggleSwitch switchh = sender as ToggleSwitch;
            switch (switchh.IsOn) {
                // imperial
                case true:
                    SettingsManager.GetDefaultSettingsManager().PutSetting("Units", Units.Imperial.ToString());
                    break;
                // metric
                case false:
                    SettingsManager.GetDefaultSettingsManager().PutSetting("Units", Units.Metric.ToString());
                    break;
            }
        }

        private void TimeFormatToggle_Toggled(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            ToggleSwitch switchh = sender as ToggleSwitch;
            switch (switchh.IsOn) {
                // 12h
                case true:
                    SettingsManager.GetDefaultSettingsManager().PutSetting("TimeFormat", TimeFormat.H12.ToString());
                    break;
                // 24h
                case false:
                    SettingsManager.GetDefaultSettingsManager().PutSetting("TimeFormat", TimeFormat.H24.ToString());
                    break;
            }
        }
    }
}
