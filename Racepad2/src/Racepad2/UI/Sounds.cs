using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            MediaElement.SetSource(OffCourseStream, OffCourseFile.ContentType);
            MediaElement.Play();
        }

        /// <summary>
        /// Play the off turn sound
        /// TODO: Add sound toggle to settings
        /// ... TODO: Add settings?!
        /// </summary>
        public void PlayTurn() {
            MediaElement.SetSource(TurnStream, TurnFile.ContentType);
            MediaElement.Play();
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
