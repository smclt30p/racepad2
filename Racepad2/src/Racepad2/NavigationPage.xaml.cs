/*
* NavigationPage - Copyright 2017 Supremacy Software
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
using System.Collections.Generic;

using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.System.Display;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Popups;

using Racepad2.Core.Navigation.Route;
using Racepad2.Navigation.Maths;
using Racepad2.UI;
using Racepad2.Core;
using Racepad2.Core.Util;
using Racepad2.UI.Controls;
using Racepad2.Core.Util.Conversions;

namespace Racepad2 {

    /// <summary>
    /// The main navigation page displaying the map.
    /// </summary>
    public sealed partial class NavigationPage : Page {

        private DisplayRequest Display { get; set; }
        private Session Session { get; set; }
        private DriveRoute Route { get; set; }
        private CourseStatus CourseStatus { get; set; }
        private Sounds Sounds { get; set; }
        private WalkingList<double> PauseDetection { get; set; }
        private int CornerOffset { get; set; } = 0;
        private bool CoursePaused { get; set; } = false;
        private CourseStatus PauseCourseStatus { get; set; }
        public static CoreDispatcher MainDispatcher { get; set; }
        private DispatcherTimer Timer { get; set; }
        private Geolocator Locator { get; set; }

        public NavigationPage() {
            InitializeComponent();
            Display = new DisplayRequest();
            Session = new Session();
            Sounds = new Sounds();
            PauseDetection = new WalkingList<double>(3);
            CourseStatus = CourseStatus.COURSE_NOT_STARTED;
            Display.RequestActive();
            InitializePauseDetection();
        }

        /// <summary>
        /// Occurs when the page is loaded. When the page loads,
        /// the route is loaded and displayed on the map if there
        /// was a route provided via Frame.Navigate()
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            /* Attach back listener to handle the back press */
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

        /// <summary>
        /// Initializes the Pause detection algorithm
        /// </summary>
        private void InitializePauseDetection() {
            for (int i = 0; i < 5; i++) {
                PauseDetection.Add(Double.MaxValue);
            }
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
            Locator = new Geolocator() {
                DesiredAccuracyInMeters = 1
            };
            Timer = new DispatcherTimer();
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
        }

        /// <summary>
        /// This method is the main trigger for the logic, this method is the callback
        /// of the time. This method is called every second.
        /// </summary>
        private async void Timer_Tick(object sender, object e) {
            try {
                Geoposition position = await Locator.GetGeopositionAsync(new TimeSpan(0, 0, 2), new TimeSpan(0, 0, 1));
                ProcessLocation(position);
            } catch (Exception) {
                UpdateInstruction("Searching for GPS...");
            }
            
        }

        /// <summary>
        /// Main logic of the navigation system, this is the callback called in <see cref="StartNavigation"/>
        /// every second providing the geographical data to process it here.
        /// </summary>
        /// <param name="args">The current position snapshot</param>
        private void ProcessLocation(Geoposition args) {
            BasicGeoposition currentLocation = args.Coordinate.Point.Position;
            switch (CourseStatus) {
                /* This occurs when the 
                 * vehicle has finished the course
                 */
                case CourseStatus.COURSE_FINISHED:
                    if (!IsVehicleMoving(args)) break;
                    UpdateInstruction("");
                    break;
                /* This occurs when the vehicle is off of
                 * the course. Thanks Google for the algorithm!
                 */
                case CourseStatus.COURSE_OFF_COURSE:
                    UpdateInstruction("Off course!");
                    PlayOffCourseSound();
                    if (!IsVehicleMoving(args)) break;
                    if (!IsVehicleOffCourse(currentLocation, Session.Path)) {
                        CourseStatus = CourseStatus.COURSE_IN_PROGRESS;
                        Sounds.ResetOffCoursePlayed();
                        break;
                    }
                    break;
                case CourseStatus.COURSE_PAUSED:
                    UpdateInstruction("Course paused");
                    if (IsVehicleMoving(args)) {
                        CourseStatus = PauseCourseStatus;
                    }
                    return; /* Prevent map and data from updating */
                /* This occurs when the vehicle has 
                 * passed the final corner and is heading
                 * tovards the finish
                 */
                case CourseStatus.COURSE_LAST_STRAIGHT:
                    if (!IsVehicleMoving(args)) break;
                    double courseEnd = GeoMath.Distance(Route.Path[Route.Path.Count - 1], currentLocation);
                    UpdateInstruction(String.Format("Course ends in {0}m", Math.Round(courseEnd, 0)));
                    if (VehicleIsNearFinish(courseEnd)) {
                        CourseStatus = CourseStatus.COURSE_FINISHED;
                    }
                    break;
                /* This occurs when the vehicle has not started the course */
                case CourseStatus.COURSE_NOT_STARTED:
                    UpdateInstruction("Head towards start of course");
                    if (!IsVehicleMoving(args)) break;
                    if (IsVehicleNearCourseStart(currentLocation)) {
                        UpdateInstruction("Course started");
                        CourseStatus = CourseStatus.COURSE_IN_PROGRESS;
                    }
                    break;
                /* This occurs when the vehicle is on course
                 * and the course is started and not finished 
                 */
                case CourseStatus.COURSE_IN_PROGRESS:
                    if (!IsVehicleMoving(args)) {
                        CourseStatus = CourseStatus.COURSE_PAUSED;
                        break;
                    }
                    if (IsVehicleOffCourse(currentLocation, Session.Path)) {
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
            Session.CurrentPosition = new VehiclePosition(args.Coordinate.Point, args.Coordinate.Heading);
            Map.Position = Session.CurrentPosition;
            Map.PreviewPath = Session.Path;
            UpdateView(args);
        }

        /// <summary>
        /// Detects if the vehicle is less than 10 meters away from the start
        /// of the course from all directions.
        /// </summary>
        /// <param name="currentLocation">The vehicle's current location</param>
        /// <returns>True if vehicle is less than 10m away from the start</returns>
        private bool IsVehicleNearCourseStart(BasicGeoposition currentLocation) {
            return GeoMath.Distance(currentLocation, Route.Path[0]) < 10;
        }

        /// <summary>
        /// When a user clears a corner, this method gets invoked. This method
        /// displays the messages when a corner is approaching and displays
        /// the "Turn XXX" message.
        /// </summary>
        /// <param name="currentLocation"></param>
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

        /// <summary>
        /// Returns true if the vehicle is within 10 meters of the course finish
        /// towards the final straight.
        /// </summary>
        /// <param name="courseEnd">The remaining l</param>
        /// <returns>True if the vehicle is within 10m of the finish</returns>
        private bool VehicleIsNearFinish(double courseEnd) {
            return courseEnd < 10;
        }
       
        /// <summary>
        /// Returns true if there are no more corners left and the 
        /// vehicle is moving towards the final straight of the 
        /// course and the finish.
        /// </summary>
        private bool IsVehiclePastFinalCorner() {
            return CornerOffset + 1 > Route.Corners.Count;
        }
        
        /// <summary>
        /// Returns true if the vehicle is off of the current course.
        /// </summary>
        /// <param name="currentLocation"></param>
        /// <returns></returns>
        private bool IsVehicleOffCourse(BasicGeoposition currentLocation, List<BasicGeoposition> polyline) {
            return !PolyUtil.isLocationOnPath(currentLocation, polyline , false, 20);
        }
        
        /// <summary>
        /// Return true if the vehicle's average speed is above 3km/h in
        /// 3 seconds.
        /// </summary>
        private bool IsVehicleMoving(Geoposition args) {
            if (args.Coordinate.Speed != null && args.Coordinate.Speed != Double.NaN) {
                PauseDetection.Add((double)args.Coordinate.Speed);
            }
            bool ret = VehicleMoving();
            if (!ret) {
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
                Session.Instruction = data;
            });
        }
        
        /// <summary>
        /// Updates the data displayed in the data tiles after a position change event.
        /// 
        /// </summary>
        /// <param name="args">The position changed event argument</param>
        private async void UpdateView(Geoposition args) {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (args.Coordinate.Speed != null && !Double.IsNaN((double)args.Coordinate.Speed)) {
                    Session.Speed = (double)args.Coordinate.Speed;
                    Session.Distance += (double)args.Coordinate.Speed;
                    Session.Time += 1;
                }
                Session.Elevation = args.Coordinate.Point.Position.Altitude;
            });
        }
        
        /// <summary>
        /// Detect if the vehicle is not moving. A vehicle is not considered
        /// moving if the average speed in 3 seconds is not more than 3km/h
        /// </summary>
        /// <returns>If the vehicle is moving</returns>
        private bool VehicleMoving() {
            double avg = 0;
            foreach (double d in PauseDetection) {
                avg += d;
            }
            return avg > 3;
        }

        /// <summary>
        /// This gets called by the main App class
        /// when the back button is pressed on the NavigationPage
        /// </summary>
        internal async void BackRequested() {
            MessageDialog dialog = new MessageDialog("Save session?");
            dialog.Title = "Exiting session";
            UICommand yes = new UICommand("Yes") {
                Id = 1
            };
            UICommand no = new UICommand("No") {
                Id = 0
            };
            dialog.Commands.Add(yes);
            dialog.Commands.Add(no);
            IUICommand res = await dialog.ShowAsync();
            if ((int)res.Id == 1) {
                SaveAndExit();
            } else {
                ExitNavigation();
            }
        }

        /// <summary>
        /// Occurs when the user confirms that he wants to leave the navigation
        /// saving the session
        /// </summary>
        private async void SaveAndExit() {
            Session.Close();
            TextInputDialog dialog = new TextInputDialog("Enter a session name");
            await dialog.ShowAsync();
            if (dialog.InnerText == null) {
                Session.Name = TimeConversions.ISOTimestamp(Session.StartTime);
            } else {
                Session.Name = dialog.InnerText;
            }
            SettingsManager manager = SettingsManager.GetDefaultSettingsManager();
            List<Session> sessions = (List<Session>) manager.ReadList<Session>("Sessions");
            if (sessions == null) {
                sessions = new List<Session>();
            }
            sessions.Add(Session);
            manager.WriteList<Session>("Sessions", sessions);
            ExitNavigation();
        }

        /// <summary>
        /// Occurs when the user confirms that he wants to leave the navigation
        /// without saving the session
        /// </summary>
        private void ExitNavigation() {
            Frame.Navigate(typeof(MainPage));
        }
    }
}