/*
* BasicStatusTile - Copyright 2017 Supremacy Software
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
using Windows.UI.Xaml.Controls;

namespace Racepad2.UI.StatusTiles {

    /// <summary>
    /// This presents a basic status tile with a small
    /// description and the master text inside of it.
    /// 
    /// Specific tiles are supposed to inherit this 
    /// class and add view conversion for raw values
    /// that are provided via the Value property of this
    /// class.
    /// </summary>
    public abstract partial class BasicStatusTile : UserControl {

        /// <summary>
        /// This property holds the status tile's
        /// master value.
        /// </summary>
        public virtual object Value {
            get {
                return DisplayValue.Text;
            }
            set {
                DisplayValue.Text = value.ToString();
            }
        }

        /// <summary>
        /// This property holds the status tile's
        /// small description.
        /// </summary>
        public string BottomText {
            get {
               return DisplayDescription.Text;
            }
            set {
                DisplayDescription.Text = value;
            }
        }

        public BasicStatusTile() {
            this.InitializeComponent();
            this.SizeChanged += Grid_SizeChanged;
        }

        /// <summary>
        /// This method is used to optimize the font size of the visual controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e) {
            ResizeFont();
        }

        /// <summary>
        /// If the contro's font size is too big it gets reduced by 5 points
        /// </summary>
        private void ResizeFont() {
            double displaywidth = DisplayValue.ActualWidth;
            double containerwidth = base.ActualWidth;
            if (displaywidth + 20 > containerwidth) {
                DisplayValue.FontSize -= 5;
            }
        }
    }
}
