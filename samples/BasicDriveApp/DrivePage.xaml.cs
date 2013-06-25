using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

using RobotKit;

namespace BasicDriveApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //! @brief  the default string to show no sphero connected
        private const string kNoSpheroConnected = "No Sphero Connected";

        //! @brief  the default string to show when connecting to a sphero ({0})
        private const string kConnectingToSphero = "Connecting to {0}";

        //! @brief  the default string to show when connected to a sphero ({0})
        private const string kSpheroConnected = "Connected to {0}";


        //! @brief  the robot we're connecting to
        Sphero m_robot = null;

        //! @brief  the joystick to control m_robot
        private Joystick m_joystick;

        //! @brief  the color wheel to control m_robot color
        private ColorWheel m_colorwheel;

        //! @brief  the calibration wheel to calibrate m_robot
        private CalibrateElement m_calibrateElement;

        public MainPage() {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);

            SetupRobotConnection();
            Application app = Application.Current;
            app.Suspending += OnSuspending;
        }

        /*!
         * @brief   handle the user launching this page in the application
         * 
         *  connects to sphero and sets up the ui
         */
        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            base.OnNavigatedFrom(e);

            ShutdownRobotConnection();
            ShutdownControls();

            Application app = Application.Current;
            app.Suspending -= OnSuspending;
        }

        //! @brief  handle the application entering the background
        private void OnSuspending(object sender, SuspendingEventArgs args) {
            ShutdownRobotConnection();
        }

        //! @brief  search for a robot to connect to
        private void SetupRobotConnection() {
            SpheroName.Text = kNoSpheroConnected;

            RobotProvider provider = RobotProvider.GetSharedProvider();
            provider.DiscoveredRobotEvent += OnRobotDiscovered;
            provider.NoRobotsEvent += OnNoRobotsEvent;
            provider.ConnectedRobotEvent += OnRobotConnected;
            provider.FindRobots();
        }

        //! @brief  disconnect from the robot and stop listening
        private void ShutdownRobotConnection() {
            if (m_robot != null) {
                m_robot.SensorControl.StopAll();
                m_robot.Sleep();
                // temporary while I work on Disconnect.
                //m_robot.Disconnect();
                ConnectionToggle.OffContent = "Disconnected";
                SpheroName.Text = kNoSpheroConnected;

                m_robot.SensorControl.AccelerometerUpdatedEvent -= OnAccelerometerUpdated;
                m_robot.SensorControl.GyrometerUpdatedEvent -= OnGyrometerUpdated;

                m_robot.CollisionControl.StopDetection();
                m_robot.CollisionControl.CollisionDetectedEvent -= OnCollisionDetected;

                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.DiscoveredRobotEvent -= OnRobotDiscovered;
                provider.NoRobotsEvent -= OnNoRobotsEvent;
                provider.ConnectedRobotEvent -= OnRobotConnected;
            }
        }

        //! @brief  configures the various sphero controls
        private void SetupControls() {
            m_colorwheel = new ColorWheel(ColorPuck, m_robot);
            m_joystick = new Joystick(Puck, m_robot);

            m_calibrateElement = new CalibrateElement(
                CalibrateRotationRoot,
                CalibrateTarget,
                CalibrateRingOuter,
                CalibrateRingMiddle,
                CalibrateRingInner,
                CalibrationFingerPoint,
                m_robot);
        }

        //! @brief  shuts down the various sphero controls
        private void ShutdownControls() {
            // I'm pretty sure this does nothing, we should just write modifiers - PJM
            m_joystick = null;
            m_colorwheel = null;
            m_calibrateElement = null;
        }

        //! @brief  when a robot is discovered, connect!
        private void OnRobotDiscovered(object sender, Robot robot) {
            Debug.WriteLine(string.Format("Discovered \"{0}\"", robot.BluetoothName));

            if (m_robot == null) {
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.ConnectRobot(robot);
                ConnectionToggle.OnContent = "Connecting...";
                m_robot = (Sphero)robot;
                SpheroName.Text = string.Format(kConnectingToSphero, robot.BluetoothName);
            }
        }


        private void OnNoRobotsEvent(object sender, EventArgs e) {
            MessageDialog dialog = new MessageDialog("No Sphero Paired");
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            dialog.ShowAsync();
        }


        //! @brief  when a robot is connected, get ready to drive!
        private void OnRobotConnected(object sender, Robot robot) {
            Debug.WriteLine(string.Format("Connected to {0}", robot));
            ConnectionToggle.IsOn = true;
            ConnectionToggle.OnContent = "Connected";

            m_robot.SetRGBLED(255, 255, 255);
            SpheroName.Text = string.Format(kSpheroConnected, robot.BluetoothName);
            SetupControls();

            m_robot.SensorControl.Hz = 10;
            m_robot.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
            m_robot.SensorControl.GyrometerUpdatedEvent += OnGyrometerUpdated;

            m_robot.CollisionControl.StartDetectionForWallCollisions();
            m_robot.CollisionControl.CollisionDetectedEvent += OnCollisionDetected;
        }


        private void ConnectionToggle_Toggled(object sender, RoutedEventArgs e) {
            Debug.WriteLine("Connection Toggled : " + ConnectionToggle.IsOn);
            ConnectionToggle.OnContent = "Connecting...";
            if (ConnectionToggle.IsOn) {
                if (m_robot == null) {
                    SetupRobotConnection();
                }
            } else {
                ShutdownRobotConnection();
            }
        }

        private void OnAccelerometerUpdated(object sender, AccelerometerReading reading) {
            AccelerometerX.Text = "" + reading.X;
            AccelerometerY.Text = "" + reading.Y;
            AccelerometerZ.Text = "" + reading.Z;
        }

        private void OnGyrometerUpdated(object sender, GyrometerReading reading) {
            GyroscopeX.Text = "" + reading.X;
            GyroscopeY.Text = "" + reading.Y;
            GyroscopeZ.Text = "" + reading.Z;
        }

        private void OnCollisionDetected(object sender, CollisionData data) {
            Debug.WriteLine("Wall collision was detected");
        }

    }
}
