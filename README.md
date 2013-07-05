Sphero-Win-SDK
==============

![sphero-apps.jpg](https://raw.github.com/orbotix/Sphero-Win-SDK/master/images/sphero-apps.jpg)

# Windows 8.1 Quick Start Guide

## Overview

This project shows Windows 8.1 developers how to integrate Sphero into their Apps and games!

# Contents
  - Adding the RobotKit.dll to your Visual Studio solution
  - Discovering & Connecting Sphero
  - Changing Sphero's color
  - Using the Sphero UI elements
  - Driving Sphero
  - Sensor Streaming

## Adding RobotKit.dll  

From within your solution on Visual Studio 2013
    <ol>
    <li>Right click on References and "Add Reference"</li>
    <li>Choose "Browse" and select the RobotKit.dll</li>
    </ol>

## Update Manifest
    <wb:DeviceCapability Name="bluetooth.rfcomm">
      <wb:Device Id="any">
        <wb:Function Type="serviceId:00001101-0000-1000-8000-00805F9B34FB" />
      </wb:Device>
    </wb:DeviceCapability>

## Discovering & Connecting Sphero

    private void setupRobot(){
        RobotProvider provider = RobotProvider.GetSharedProvider();
        provider.DiscoveredRobotEvent += OnRobotDiscovered;
        provider.NoRobotsEvent += OnNoRobotsEvent;
        provider.ConnectedRobotEvent += OnRobotConnected;
        provider.FindRobots();
    }
  
Handle the Events...
  
    private void OnRobotDiscovered(object sender, Robot robot) {
        Debug.WriteLine(string.Format("Discovered \"{0}\"", robot.BluetoothName));

        if (m_robot == null) {
            RobotProvider provider = RobotProvider.GetSharedProvider();
            provider.ConnectRobot(robot);
            //...
        }
    }

    private void OnNoRobotsEvent(object sender, EventArgs e) {
      MessageDialog dialog = new MessageDialog("No Sphero Paired");
      //...
    }

## Changing Sphero's color

Sphero's main LED colors are controlled by web standard RGB.  Best practice is to use hex values to stay within the valid range of 0-255.

    public void turnSpheroWhite(){
        int red = 0xFF;
        int green = 0xFF
        int blue = 0xFF
        m_robot.SetRGBLED(red, green, blue);
    }

## Using the Sphero UI Elements

From within the BasicDriveApp sample, copy the RobotKitUI code for the Joystick and Calibration controls.  Sphero needs to be calibrated so that the 'back LED" is aimed toward the driver.  The easiest way to accomplish this is to use the 'CalibrateElement.cs'.

## Driving Sphero

Driving Sphero is super simple.  Just give it a direction in degrees where 0 is directly ahead and increases clockwise where 90 degrees is right and a velocity between 0.0 and 1.0.

    public void hangRightAtHalfSpeed(){
        int heading = 90; // right
        float velocity = .5; // half speed
        m_sphero.Roll(heading, velocity);
    }

## Sensor Streaming

Sphero is capable of being used as a simple game controller or other input device using it's embedded sensors.  Sphero has the following sensors.

<ul>
<li>Accelerometer</li>
<li>Gyrometer</li>
<li>Location in X,Y from a relative starting point</li>
<li>Velocity</li>
<li>Attitude in both Quaternions and Euler angles</li>
</ul>

### Setup
  
The robot SensorControl manages the listeners for Accelerometer, Gyro, Location, Attitude in Eulers and Quaternion Attitude.  The `SensorControl.Hz` value is between 1 and 400 and affects all sensors.

    {
      m_robot.SensorControl.Hz = 60; // stream at 60Hz for ALL sensors that are enabled
      m_robot.SensorControl.AccelerometerUpdatedEvent += OnAccelerometerUpdated;
    }

### Data Handler

    private void OnAccelerometerUpdated(object sender, AccelerometerReading reading) {
    // expects AccelerometerX,Y,Z to be defined as fields
        AccelerometerX.Text = "" + reading.X;
        AccelerometerY.Text = "" + reading.Y;
        AccelerometerZ.Text = "" + reading.Z;
    }

# Samples
