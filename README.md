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

## Driving Sphero

    public void hangRightAtHalfSpeed(){
        int heading = 90; // right
        float velocity = .5; // half speed
        m_sphero.Roll(heading, velocity);
    }

## Sensor Streaming

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
