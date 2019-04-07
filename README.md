# Unity Autonomous Vehicle Simulator with ROS Integration

![](images/mountain-road.PNG?raw=true)

Based on assets from the [Udacity Self Driving Car Sim](https://github.com/udacity/self-driving-car-sim)

## Background

This Unity3D vehicle simulator was built by the Texas A&M AutoDrive Simulation Team for the 2017-2018 SAE AutoDrive competition to more safely develop vehicle control software while reducing the need to schedule additional field testing sessions.

### Simulation Team Members
* Steven Leal - project lead and developer
* Juan Vasquez - developer
* Brandon Gann - Field testing callaborator
* Key Kim - Field testing callaborator

## Key Features

### ROS Integration

The most important feature of the simulator is its ROS integration. Connecting the simulator to ROS allows a network of control nodes to be executed on their native platform while being tested in a safe and convenient virtual environment. To facilitate this integration, the simulator connects to a ROS master node on the network and streams data from a variety of sensors to other subscribed nodes. These ROS compute nodes can then use this published sensor information to make decisions and ultimately publish messages back to the simulator to control the virtual vehicle. Below is an example of the simulator publishing front camera information to a lane detection ROS node. The lane detection node can also send live rendered outputs through ROS back to the simulator for monitoring.

![](images/lane-detection.PNG?raw=true)

### Simulator Features

The simulator itself contains a variety of useful features highlighted by its array of virtual sensors. These include configurable camera outputs, IMU data, and internal vehicle state information such as throttle and steering data.

In the main menu, additional simulator parameters can be adjusted, including:
* Simulation Speed
* Varying degrees of auto pilot
* The designated driving lane
* Publish rate of output messages

![](images/options.PNG?raw=true)

The level of customization a simulator provides particularly shines when prototyping sensor configurations. Tasks such as choosing where to mount cameras around the vehicle or testing those cameras with fish eye lens would normally take hours to days to customize in reality but only take minutes to prototype in the simulation.

![](images/fish-eye-camera.PNG?raw=true)

### Road Builder Gizmo

The final key feature of the simulator is a spline based road creation tool that generates smooth looped tracks with customizable shapes and textures.

![](images/road-builder.PNG?raw=true)

GPS waypoints are computed along the generated lanes for guiding the simulator's auto pilot feature in addition to providing simulated GPS data for use in external ROS controller nodes.

![](images/way-points.PNG?raw=true)
