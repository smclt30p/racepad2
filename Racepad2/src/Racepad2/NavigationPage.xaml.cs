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
        private bool CoursePaused { get; set; } = false;
        private CourseStatus PauseCourseStatus { get; set; }

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
            GPXRouteParser parser = new GPXRouteParser("<gpx xmlns=\"http://www.topografix.com/GPX/1/1\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd\" version=\"1.1\" creator=\"OpenRouteService.org\"><metadata/><trk><name></name><desc></desc><trkseg><trkpt lat=\"44.855448\" lon=\"17.236793\"><ele>133</ele></trkpt><trkpt lat=\"44.855442\" lon=\"17.236855\"><ele>133</ele></trkpt><trkpt lat=\"44.855325\" lon=\"17.237243\"><ele>133</ele></trkpt><trkpt lat=\"44.855177\" lon=\"17.237559\"><ele>133.3</ele></trkpt><trkpt lat=\"44.855373\" lon=\"17.237799\"><ele>133.4</ele></trkpt><trkpt lat=\"44.855788\" lon=\"17.238239\"><ele>133.6</ele></trkpt><trkpt lat=\"44.855841\" lon=\"17.238233\"><ele>133.6</ele></trkpt><trkpt lat=\"44.856009\" lon=\"17.237999\"><ele>133.7</ele></trkpt><trkpt lat=\"44.856125\" lon=\"17.237845\"><ele>133.7</ele></trkpt><trkpt lat=\"44.856158\" lon=\"17.237801\"><ele>133.7</ele></trkpt><trkpt lat=\"44.85634\" lon=\"17.237645\"><ele>133.8</ele></trkpt><trkpt lat=\"44.856511\" lon=\"17.237452\"><ele>134</ele></trkpt><trkpt lat=\"44.856629\" lon=\"17.237232\"><ele>134.2</ele></trkpt><trkpt lat=\"44.856975\" lon=\"17.236851\"><ele>134.6</ele></trkpt><trkpt lat=\"44.85726\" lon=\"17.236583\"><ele>135</ele></trkpt><trkpt lat=\"44.857462\" lon=\"17.236438\"><ele>135.2</ele></trkpt><trkpt lat=\"44.85758\" lon=\"17.236326\"><ele>135.2</ele></trkpt><trkpt lat=\"44.857659\" lon=\"17.236324\"><ele>135.3</ele></trkpt><trkpt lat=\"44.857743\" lon=\"17.236484\"><ele>135.3</ele></trkpt><trkpt lat=\"44.857788\" lon=\"17.236573\"><ele>135.4</ele></trkpt><trkpt lat=\"44.857996\" lon=\"17.236982\"><ele>135.7</ele></trkpt><trkpt lat=\"44.858056\" lon=\"17.237099\"><ele>135.7</ele></trkpt><trkpt lat=\"44.858211\" lon=\"17.237403\"><ele>135.8</ele></trkpt><trkpt lat=\"44.858433\" lon=\"17.237856\"><ele>136</ele></trkpt><trkpt lat=\"44.85877\" lon=\"17.238532\"><ele>136.6</ele></trkpt><trkpt lat=\"44.858697\" lon=\"17.238571\"><ele>136.8</ele></trkpt><trkpt lat=\"44.858419\" lon=\"17.238716\"><ele>137.2</ele></trkpt><trkpt lat=\"44.858308\" lon=\"17.238751\"><ele>137.2</ele></trkpt><trkpt lat=\"44.85817\" lon=\"17.238769\"><ele>137.3</ele></trkpt><trkpt lat=\"44.857636\" lon=\"17.238766\"><ele>137.2</ele></trkpt><trkpt lat=\"44.857317\" lon=\"17.238868\"><ele>136.8</ele></trkpt><trkpt lat=\"44.857119\" lon=\"17.239073\"><ele>136.4</ele></trkpt><trkpt lat=\"44.856946\" lon=\"17.239374\"><ele>135.9</ele></trkpt><trkpt lat=\"44.856753\" lon=\"17.239778\"><ele>135.5</ele></trkpt><trkpt lat=\"44.85668\" lon=\"17.239916\"><ele>135.3</ele></trkpt><trkpt lat=\"44.856637\" lon=\"17.239951\"><ele>135.2</ele></trkpt><trkpt lat=\"44.856585\" lon=\"17.239916\"><ele>135.2</ele></trkpt><trkpt lat=\"44.856166\" lon=\"17.239346\"><ele>135.2</ele></trkpt><trkpt lat=\"44.855999\" lon=\"17.239169\"><ele>135.3</ele></trkpt><trkpt lat=\"44.855729\" lon=\"17.238928\"><ele>135.5</ele></trkpt><trkpt lat=\"44.855523\" lon=\"17.238799\"><ele>135.6</ele></trkpt><trkpt lat=\"44.855405\" lon=\"17.238826\"><ele>135.6</ele></trkpt><trkpt lat=\"44.855352\" lon=\"17.238874\"><ele>135.6</ele></trkpt><trkpt lat=\"44.85525\" lon=\"17.239072\"><ele>135.6</ele></trkpt><trkpt lat=\"44.855181\" lon=\"17.239196\"><ele>135.6</ele></trkpt><trkpt lat=\"44.854717\" lon=\"17.238777\"><ele>135.1</ele></trkpt><trkpt lat=\"44.854467\" lon=\"17.238468\"><ele>134.8</ele></trkpt><trkpt lat=\"44.853812\" lon=\"17.237706\"><ele>134.5</ele></trkpt><trkpt lat=\"44.853613\" lon=\"17.238205\"><ele>134.5</ele></trkpt><trkpt lat=\"44.8534\" lon=\"17.238706\"><ele>134.7</ele></trkpt><trkpt lat=\"44.853221\" lon=\"17.239194\"><ele>134.8</ele></trkpt><trkpt lat=\"44.852749\" lon=\"17.240392\"><ele>135</ele></trkpt></trkseg></trk></gpx>");
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

            Map.UpdateMapPosition(args.Position.Coordinate.Point, args.Position.Coordinate.Heading);
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
