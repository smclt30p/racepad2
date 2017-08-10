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
using Windows.UI.Xaml.Navigation;

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
        private bool CoursePaused { get; set; } = false;
        private CourseStatus PauseCourseStatus { get; set; }
        private VehiclePosition Position { get; set; }
        public static CoreDispatcher MainDispatcher { get; set; }

        public NavigationPage() {
            InitializeComponent();
            Map.ViewDispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            Display = new DisplayRequest();
            Ride = new Ride();
            Sounds = new Sounds();
            PauseDetection = new WalkingList<double>(3);
            Position = new VehiclePosition();
            CourseStatus = CourseStatus.COURSE_NOT_STARTED;
            LoadSettings();
            Display.RequestActive();
            InitializePauseDetection();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {

            if (e.Parameter == null) {
                CourseStatus = CourseStatus.COURSE_FINISHED;
            } else {
                if (e.Parameter is DriveRoute) {
                    DriveRoute route = e.Parameter as DriveRoute;
                    Map.Route = route;
                    Route = route;
                } else {
                    throw new Exception("Navigation parameter is not a route!");
                }
            }
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

            BasicGeoposition currentLocation = args.Position.Coordinate.Point.Position;

            switch (CourseStatus) {

                /* This occurs when the 
                 * vehicle has finished the course
                 */
                case CourseStatus.COURSE_FINISHED:
                    if (IsVehicleNotMoving(args)) break;

                    UpdateInstruction("");
                    break;
                /* This occurs when the vehicle is off of
                 * the course. Thanks Google for the algorithm!
                 */
                case CourseStatus.COURSE_OFF_COURSE:
                    UpdateInstruction("Off course!");
                    PlayOffCourseSound();
                    if (IsVehicleNotMoving(args)) break;
                    if (!IsVehicleOffCourse(currentLocation)) {
                        CourseStatus = CourseStatus.COURSE_IN_PROGRESS;
                        Sounds.ResetOffCoursePlayed();
                        break;
                    }
                    break;
                case CourseStatus.COURSE_PAUSED:
                    UpdateInstruction("Course paused");
                    if (!IsVehicleNotMoving(args)) { 
                        CourseStatus = PauseCourseStatus;
                    }
                    return; /* Prevent map and data from updating */
                /* This occurs when the vehicle has 
                 * passed the final corner and is heading
                 * tovards the finish
                 */
                case CourseStatus.COURSE_LAST_STRAIGHT:
                    if (IsVehicleNotMoving(args)) break;
                    double courseEnd = GeoMath.Distance(Route.Path[Route.Path.Count - 1], currentLocation);
                    UpdateInstruction(String.Format("Course ends in {0}m", Math.Round(courseEnd, 0)));
                    if (VehicleIsNearFinish(courseEnd)) {
                        CourseStatus = CourseStatus.COURSE_FINISHED;
                    }
                    break;
                /* This occurs when the vehicle has not started the course */
                case CourseStatus.COURSE_NOT_STARTED:
                    UpdateInstruction("Head towards start of course");
                    if (IsVehicleNotMoving(args)) break;
                    if (IsVehicleNearCourseStart(currentLocation)) {
                        UpdateInstruction("Course started");
                        CourseStatus = CourseStatus.COURSE_IN_PROGRESS;
                    }
                    break;
                /* This occurs when the vehicle is on course
                 * and the course is started and not finished 
                 */
                case CourseStatus.COURSE_IN_PROGRESS:

                    if (IsVehicleNotMoving(args)) {
                        CourseStatus = CourseStatus.COURSE_PAUSED;
                        break;
                    }
                    if (IsVehicleOffCourse(currentLocation)) {
                        CourseStatus = CourseStatus.COURSE_OFF_COURSE;
                        break;
                    }
                    if (IsVehiclePastFinalCorner()) {
                        CourseStatus = CourseStatus.COURSE_LAST_STRAIGHT;
                        break;
                    }
                    NextCorner(currentLocation);
                    break;
                    
            }

            Position.Position = args.Position.Coordinate.Point;
            Position.Bearing = args.Position.Coordinate.Heading;

            Map.Position = Position;
            UpdateView(args);        

        }

        private bool IsVehicleNearCourseStart(BasicGeoposition currentLocation) {
            return GeoMath.Distance(currentLocation, Route.Path[0]) < 10;
        }

        private void NextCorner(BasicGeoposition currentLocation) {

            double upcomingCorner = GeoMath.Distance(Route.Corners[CornerOffset].Position, currentLocation);

            if (upcomingCorner > 400) {
                UpdateInstruction("");
            } else if (upcomingCorner > 50) {
                UpdateInstruction(String.Format("In {0}m turn {1}", Math.Round(upcomingCorner, 0),
                    Corner.GetDescriptionForCorner(Route.Corners[CornerOffset])));
            } else if (upcomingCorner > 20) {
                UpdateInstruction("Turn " + Corner.GetDescriptionForCorner(Route.Corners[CornerOffset]));
                PlayTurnSound();
            }

            /* Corner is less than 20m away, turn in and  switch to next one */
            if (upcomingCorner < 20) {
                CornerOffset++;
                Sounds.ResetTurnPlayed();
            }

        }

        private bool VehicleIsNearFinish(double courseEnd) {
            return courseEnd < 10;
        }

        private bool IsVehiclePastFinalCorner() {
            return CornerOffset + 1 > Route.Corners.Count;
        }

        private bool IsVehicleOffCourse(BasicGeoposition currentLocation) {
            return !PolyUtil.isLocationOnPath(currentLocation, Route.Path, false, 20);
        }

        private bool IsVehicleNotMoving(PositionChangedEventArgs args) {
            if (args.Position.Coordinate.Speed != null && args.Position.Coordinate.Speed != Double.NaN) {
                PauseDetection.Add((double)args.Position.Coordinate.Speed);
            }
            bool ret = VehicleNotMoving();

            if (ret) {
                if (CourseStatus != CourseStatus.COURSE_PAUSED) {
                    PauseCourseStatus = CourseStatus;
                }
                CourseStatus = CourseStatus.COURSE_PAUSED;
            }

            return ret;
        }

        /// <summary>
        /// Play the turn audio signal. This is played 20 meters
        /// before a turn occurs and plays once.
        /// </summary>
        private async void PlayTurnSound() {

            /* Don't play the sound a second time */
            /* Switch to UI thread, Android equivavlent of runOnUiThread() */
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                Sounds.PlayTurn();
            });

        }

        /// <summary>
        /// Play the off course sound. This is played when off the GPX course
        /// and is played each second while off course */
        /// </summary>
        private async void PlayOffCourseSound() {

            /*Don't play the off course sound again */
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
        private bool VehicleNotMoving() {

            double avg = 0;
            foreach (double d in PauseDetection) {
                avg += d;
            }
            return avg < 3;

        }
     
    }
}
