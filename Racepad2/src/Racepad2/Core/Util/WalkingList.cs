/*
* WalkingList - Copyright 2017 Supremacy Software
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


namespace Racepad2.Core {

    /// <summary>
    /// Represents a reversed fixed-size queue
    /// </summary>
    /// <typeparam name="T">The type of items that the reversed fixed-size queue holds</typeparam>
    class WalkingList<T> : List<T> {

        /// <summary>
        /// Default queue size is 10 items
        /// </summary>
        private int THRESHOLD = 10;

        public WalkingList() { }

        /// <summary>
        /// Create a new fixed reversed queue with the specified size
        /// </summary>
        /// <param name="threshold"></param>
        public WalkingList(int threshold) {
            this.THRESHOLD = threshold;
        }

        /// <summary>
        /// Put a new item at the end of the queue. If there is >THRESHOLD items,
        /// the first item in the list will be removed.
        /// </summary>
        /// <param name="element"></param>
        public new void Add(T element) {
            if (base.Count >= THRESHOLD) {
                base.RemoveAt(0);
            }
            base.Add(element);
        }

    }
}
