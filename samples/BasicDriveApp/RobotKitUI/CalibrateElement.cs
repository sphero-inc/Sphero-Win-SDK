using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace BasicDriveApp
{
    class CalibrateElement
    {
        /*!
         * @brief  the uielement that rotates to indicate calibration - should be the parent of
         *          whatever element the user touches to rotate
         */
        private FrameworkElement m_calibrateRotationRoot;

        //! @brief  an element the user can touch to initiate calibration
        private FrameworkElement m_calibrateElement;

        //! @brief	calibrate rings
        private FrameworkElement m_ringOuter;
        private FrameworkElement m_ringMiddle;
        private FrameworkElement m_ringInner;

        //! @brief	calibrate finger point
        private FrameworkElement m_fingerPoint;

        //! @brief  a rotation transform applied to m_calibrateRotationRoot
        private RotateTransform m_rotation;

        //! @brief  the sphero we can calibrate
        private RobotKit.Sphero m_sphero;

        //! @brief  the last time a command was sent in milliseconds
        private long m_lastCommandSentTimeMs;

        /*!
         * Initializes this calibration element
         * @param   rotationElement the element that rotates (can have children that travel with it)
         * @param   element the element that is touched to begin rotation, will track the finger
         * @param   the outer ring image
         * @param   the middle ring image
         * @param   the inner ring image
         * @param   sphero the sphero that will be calibrated
         */
        public CalibrateElement(
            FrameworkElement rotationElement,
            FrameworkElement element,
            FrameworkElement ringOuter,
            FrameworkElement ringMiddle,
            FrameworkElement ringInner,
            FrameworkElement fingerPoint,
            RobotKit.Robot sphero) {
            m_sphero = (RobotKit.Sphero)sphero;
            m_calibrateRotationRoot = rotationElement;
            m_calibrateElement = element;

            m_calibrateElement.PointerPressed += OnPointerPressed;
            m_calibrateElement.PointerMoved += OnPointerMoved;
            m_calibrateElement.PointerReleased += OnPointerReleased;
            m_calibrateElement.PointerCaptureLost += OnPointerReleased;
            m_calibrateElement.PointerCanceled += OnPointerReleased;

            m_ringOuter = ringOuter;
            m_ringMiddle = ringMiddle;
            m_ringInner = ringInner;

            m_fingerPoint = fingerPoint;

            SetupRotation();

            m_lastCommandSentTimeMs = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        //! @brief  sets up the rotation transform
        private void SetupRotation() {
            m_rotation = new RotateTransform();
            m_calibrateRotationRoot.RenderTransform = m_rotation;
            m_rotation.CenterX = m_calibrateRotationRoot.ActualWidth / 2;
            m_rotation.CenterY = m_calibrateRotationRoot.ActualHeight / 2;
        }

        /*!
         * @brief	calibrate from the given @a point in relationship to m_calibrateRotationRoot
         * @param   point the point relative to the rotation root
         */
        private void CalibrateFromPoint(Point point) {
            float width = (float)m_calibrateRotationRoot.ActualWidth;
            float height = (float)m_calibrateRotationRoot.ActualHeight;

            // move origin from the top left to the center with y-up and x-right
            Matrix matrix = new Matrix(
                1, 0,
                0, -1,
                -width / 2.0, height / 2.0);
            Point localPoint = matrix.Transform(point);

            float angle = (float)Math.Atan2(localPoint.Y, localPoint.X);
            int angleDegrees = (int)(angle * 180.0 / Math.PI);
            angleDegrees += 90;
            angleDegrees = (angleDegrees + 360) % 360;
            angleDegrees = 360 - angleDegrees;

            // apply the rotation to the ui
            m_rotation.Angle = angleDegrees;

            // preview the calibration and limit to 10 Hz
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if ((milliseconds - m_lastCommandSentTimeMs) > 100) {
                m_sphero.Roll(angleDegrees, 0);
                m_lastCommandSentTimeMs = milliseconds;
            }
        }

        //! @brief  handle the user starting to calibrate
        private void OnPointerPressed(object sender, PointerRoutedEventArgs args) {
            Windows.UI.Input.PointerPoint pointer = args.GetCurrentPoint(m_calibrateRotationRoot);
            if (pointer.Properties.IsLeftButtonPressed) {
                if (m_sphero != null) {
                    m_sphero.SetHeading(0);
                    m_sphero.SetBackLED(1.0f);
                }

                m_calibrateElement.CapturePointer(args.Pointer);

                args.Handled = true;

                // Show rings
                m_ringOuter.Visibility = Visibility.Visible;
                m_ringMiddle.Visibility = Visibility.Visible;
                m_ringInner.Visibility = Visibility.Visible;

                m_fingerPoint.Visibility = Visibility.Visible;
            }
        }

        //! @brief  handle the user calibrating
        private void OnPointerMoved(object sender, PointerRoutedEventArgs args) {
            m_rotation.Angle = 0;
            Windows.UI.Input.PointerPoint pointer = args.GetCurrentPoint(m_calibrateRotationRoot);
            if (pointer.Properties.IsLeftButtonPressed) {
                CalibrateFromPoint(pointer.Position);
            }
        }

        //! @brief  handle the user completing calibration
        private void OnPointerReleased(object sender, PointerRoutedEventArgs args) {
            m_calibrateElement.ReleasePointerCapture(args.Pointer);

            if (m_sphero != null) {
                m_sphero.SetHeading(0);
                m_sphero.SetBackLED(0.0f);
            }

            args.Handled = true;

            // Hide rings
            m_ringOuter.Visibility = Visibility.Collapsed;
            m_ringMiddle.Visibility = Visibility.Collapsed;
            m_ringInner.Visibility = Visibility.Collapsed;

            m_fingerPoint.Visibility = Visibility.Collapsed;
        }
    }
}
