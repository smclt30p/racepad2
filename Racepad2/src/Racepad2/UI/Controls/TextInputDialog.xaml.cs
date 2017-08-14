/*
* TextInputDialog - Copyright 2017 Supremacy Software
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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Racepad2.UI.Controls {
    public sealed partial class TextInputDialog : ContentDialog {

        public string InnerText { get; set; } = null;

        public TextInputDialog(string title) {
            this.InitializeComponent();
            base.Title = title;
            Box.TextChanged += Box_TextChanged;
        }

        private void Box_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox source = (TextBox)sender;
            InnerText = source.Text;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) {
            base.Hide();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) {
            base.Hide();
        }
    }
}
