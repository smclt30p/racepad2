/*
* Sounds - Copyright 2017 Supremacy Software
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
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;


namespace Racepad2.UI {

    class Sounds {

        private MediaElement MediaElement { get; set; }
        private IRandomAccessStream OffCourseStream { get; set; }
        private IRandomAccessStream TurnStream { get; set; }
        private StorageFile OffCourseFile { get; set; }
        private StorageFile TurnFile { get; set; }

        private bool TurnSoundPlayed { get; set; } = false;
        private bool OffCourseSoundPlayed { get; set; } = false;

        public void ResetTurnPlayed() {
            TurnSoundPlayed = false;
        }

        public void ResetOffCoursePlayed() {
            OffCourseSoundPlayed = false;
        }

        public Sounds() {
            MediaElement = new MediaElement();
            InitialzieComponent();
        }

        /// <summary>
        /// Initialize all the async components of the class
        /// </summary>
        private async void InitialzieComponent() {
            TurnFile = await GetFile("Turn.wav");
            TurnStream = await GetStreamFromFile(TurnFile);
            OffCourseFile = await GetFile("OffCourse.wav");
            OffCourseStream = await GetStreamFromFile(OffCourseFile);
        }

        /// <summary>
        /// Play the off course sound
        /// TODO: Add sound toggle to settings
        /// ... TODO: Add settings?!
        /// </summary>
        public void PlayOffCourse() {
            if (OffCourseSoundPlayed) return;
            MediaElement.SetSource(OffCourseStream, OffCourseFile.ContentType);
            MediaElement.Play();
            OffCourseSoundPlayed = true;
        }

        /// <summary>
        /// Play the off turn sound
        /// TODO: Add sound toggle to settings
        /// ... TODO: Add settings?!
        /// </summary>
        public void PlayTurn() {
            if (TurnSoundPlayed) return;
            MediaElement.SetSource(TurnStream, TurnFile.ContentType);
            MediaElement.Play();
            TurnSoundPlayed = true;
        }

        /// <summary>
        /// Get a <see cref="StorageFile"/> from the Assets folder from a 
        /// file name.
        /// </summary>
        /// <param name="file"> The name of the file</param>
        /// <returns>The random access stream that can stream the file</returns>
        private async Task<StorageFile> GetFile(string file) {
            Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            return  await folder.GetFileAsync(file);
        }

        /// <summary>
        /// Get a stream from a file
        /// </summary>
        /// <param name="file">The file</param>
        /// <returns>A <see cref="IRandomAccessStream"/> that can stream the file</returns>
        private async Task<IRandomAccessStream> GetStreamFromFile(StorageFile file) {
            return await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
        }

    }
}
