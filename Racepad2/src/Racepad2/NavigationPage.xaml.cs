using System;
using System.Diagnostics;

using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.System.Display;

using Racepad2.Route;
using Racepad2.Geo;
using Racepad2.Geo.Google;
using Racepad2.UI;
using Racepad2.Geo.Navigation;
using Racepad2.Geo.Navigation.Core;
using Racepad2.Core;

namespace Racepad2 {

    /// <summary>
    /// The main navigation page displaying the map.
    /// </summary>
    public sealed partial class NavigationPage : Page
    {

        private DisplayRequest Display { get; set; }
        private Ride Ride { get; set; }
        private DriveRoute Route { get; set; }
        private CourseStatus CourseStatus { get; set; }
        private Sounds Sounds { get; set; }
        private WalkingList<double> PauseDetection { get; set; }
        private double AccelerometerOffset { get; set; }
        private int CornerOffset { get; set; } = 0;
        private bool TurnSoundPlayed { get; set; } = false;
        private bool OffCourseSoundPlayed { get; set; } = false;
        private bool CoursePaused { get; set; } = false;
        public static CoreDispatcher MainDispatcher { get; set; }

        public NavigationPage() {

            InitializeComponent();
            Map.ViewDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            Display = new DisplayRequest();
            Ride = new Ride();
            Sounds = new Sounds();
            PauseDetection = new WalkingList<double>(3);
            CourseStatus = CourseStatus.COURSE_NOT_STARTED;
            LoadSettings();
            Display.RequestActive();
            InitializePauseDetection();
            StartNavigation();

        }

        private void InitializePauseDetection() {
            for (int i = 0; i < 5; i++) {
                PauseDetection.Add(Double.MaxValue);
            }
        }

        private void LoadSettings() {

            /* Load settings using SettingsManager */

        }

        /// <summary>
        /// Starts the main navigation loop. Parses the GPX file provided
        /// and adds the route to the map, then initializes the GPS receiver
        /// and assings an event handler that handles a position change every second.
        /// </summary>
        private async void StartNavigation() {

            /* Parse a bogus GPX file for example */
            GPXRouteParser parser = new GPXRouteParser("");
            Route = await parser.ParseAsync();
            Map.Route = Route;
            /* Initialize the GPS receiver. We report every second with a desired precision
             * of 1m
             */
            GeolocationAccessStatus status = await Geolocator.RequestAccessAsync();
            if (status != GeolocationAccessStatus.Allowed) {
                UpdateInstruction("GPS Access denied!");
                return;
            }
            Geolocator locator = new Geolocator() {
                DesiredAccuracyInMeters = 1,
                ReportInterval = 1000,
            };

            locator.PositionChanged += Locator_PositionChanged;

        }

        /// <summary>
        /// Main logic of the navigation system, this is a event fired in <see cref="StartNavigation"/>
        /// every second providing the geographical data to process it here.
        /// </summary>
        /// <param name="sender">The <see cref="Geolocator"/> that was initialized in <see cref="StartNavigation"/></param>
        /// <param name="args">The argument that holds the positonal arguments</param>
        private void Locator_PositionChanged(Geolocator sender, PositionChangedEventArgs args) {

            /* Pause detection */
            if (args.Position.Coordinate.Speed != null && args.Position.Coordinate.Speed != Double.NaN) {
                Debug.WriteLine(args.Position.Coordinate.Speed);
                PauseDetection.Add((double)args.Position.Coordinate.Speed);
                CoursePaused = HasStopped();
            }
            if (CoursePaused) {
                UpdateInstruction("Ride paused");
                return;
            /* If the course is finished and paused, upon resuming the 
             * course the status displaya gets stuck at "Course paused". Fixes #3 */
            } else if (CourseStatus == CourseStatus.COURSE_FINISHED) {
                UpdateInstruction("");
            }
            Map.UpdateMapPosition(args.Position.Coordinate.Point, args.Position.Coordinate.Heading);
            UpdateView(args);
            BasicGeoposition currentLocation = args.Position.Coordinate.Point.Position;
            if (CourseStatus == CourseStatus.COURSE_NOT_STARTED) {
                UpdateInstruction("Head towards start of course");
                if (GeoMath.Distance(currentLocation, Route.Path[0]) < 10) {
                    UpdateInstruction("Course started");
                    CourseStatus = CourseStatus.COURSE_STARTED;
                }
                return;
            }
            /* If we finnished the course, there is no 
             * point in further navigation, return 
             */
            if (CourseStatus == CourseStatus.COURSE_FINISHED) return;
            /* Off-course detection algorithm by Google. Thanks Google! 
             * TODO: Implement course skip
             */
            if (!PolyUtil.isLocationOnPath(currentLocation, Route.Path, false, 20)) {
                UpdateInstruction("Off course!");
                PlayOffCourseSound();
                OffCourseSoundPlayed = true;
                return;
            } else {
                OffCourseSoundPlayed = false;
            }

            /* If the course is ending, display distance to end of course */
            if (CourseStatus == CourseStatus.COURSE_ENDING) {
                double courseEnd = GeoMath.Distance(Route.Path[Route.Path.Count - 1], currentLocation);
                UpdateInstruction(String.Format("Course ends in {0}m", Math.Round(courseEnd, 2)));
                if (courseEnd < 10) {
                    CourseStatus = CourseStatus.COURSE_FINISHED;
                    UpdateInstruction("");
                }
                return;
            }
            /* There are no more corners left, only a final straight. */
            if (CornerOffset + 1 > Route.Corners.Count) {
                CourseStatus = CourseStatus.COURSE_ENDING;
                return;
            }
            double upcomingCorner = GeoMath.Distance(Route.Corners[CornerOffset].Position, currentLocation);
            if (upcomingCorner > 400) {
                UpdateInstruction("");
            } else if (upcomingCorner > 50) {
                /* Safeguard needed to stop sound playing every second */
                TurnSoundPlayed = false;
                UpdateInstruction(String.Format("In {0}m turn {1}", Math.Round(upcomingCorner, 0), 
                    Corner.GetDescriptionForCorner(Route.Corners[CornerOffset])));
            } else if (upcomingCorner > 20) {
                UpdateInstruction("Turn " + Corner.GetDescriptionForCorner(Route.Corners[CornerOffset]));
                PlayTurnSound();
            }
            /* Corner is less than 20m away, turn in and  switch to next one */
            if (upcomingCorner < 20) {
                CornerOffset++;
            }

        }

        /// <summary>
        /// Play the turn audio signal. This is played 20 meters
        /// before a turn occurs and plays once.
        /// </summary>
        private async void PlayTurnSound() {

            /* Don't play the sound a second time */
            if (TurnSoundPlayed) return;
            /* Switch to UI thread, Android equivavlent of runOnUiThread() */
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Sounds.PlayTurn();
                TurnSoundPlayed = true;
            });

        }

        /// <summary>
        /// Play the off course sound. This is played when off the GPX course
        /// and is played each second while off course */
        /// </summary>
        private async void PlayOffCourseSound() {

            /*Don't play the off course sound again */
            if (OffCourseSoundPlayed) return;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Sounds.PlayOffCourse();
            });

        }

        /// <summary>
        /// Sets the text above the four meters in the map view.
        /// </summary>
        /// <param name="data">The text to be set</param>
        private async void UpdateInstruction(string data) {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Ride.Instruction = data;
            });

        }

        /// <summary>
        /// Updates the data displayed in the data tiles after a position change event.
        /// 
        /// </summary>
        /// <param name="args">The position changed event argument</param>
        private async void UpdateView(PositionChangedEventArgs args) {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (args.Position.Coordinate.Speed != null && !Double.IsNaN((double)args.Position.Coordinate.Speed)) {
                    Ride.Speed = (double)args.Position.Coordinate.Speed;
                    Ride.Distance += (double)args.Position.Coordinate.Speed;
                    Ride.Time += 1;
                }
                Ride.Elevation = args.Position.Coordinate.Point.Position.Altitude;
            });

        }

        /// <summary>
        /// Detect if the vehicle is not moving. A vehicle is not considered
        /// moving if the average speed in 3 seconds is not more than 3km/h
        /// </summary>
        /// <returns>If the vehicle is moving</returns>
        private bool HasStopped() {

            double avg = 0;
            foreach (double d in PauseDetection) {
                avg += d;
            }
            return avg < 3;

        }
     
    }
}
