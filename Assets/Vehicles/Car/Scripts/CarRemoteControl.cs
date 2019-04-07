using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarRemoteControl : MonoBehaviour
    {
        // car controller
        private CarController _carController;
        private RosSim _rosSim;
        private WaypointFollower _waypointFollower;
        private RoadBuilder _roadBuilder;

        // waypoints reference 
        private List<GameObject> waypoints;

        public float DistanceFromCurve = 0.0f;
        public float SteeringAngle { get; set; }
        public float Acceleration { get; set; }
        private CarInputs _carInputs;

        public enum CarPilotMode {
            Manual, Simulated, SimSteeringRosThrottle, SimThrottleRosSteering, ROS
        }

        private CarPilotMode _pilotMode = CarPilotMode.Manual;
        public CarPilotMode PilotMode {
            get { return _pilotMode; }
            set
            {
                if (_pilotMode == value)
                    return;
                _pilotMode = value;
                if (_pilotMode == CarPilotMode.Simulated)
                    SwitchToSimulatedDriving();
                else if (_pilotMode == CarPilotMode.SimSteeringRosThrottle)
                    SwitchToSimSteeringRosThrottle();
                else if (_pilotMode == CarPilotMode.SimThrottleRosSteering)
                    SwitchToSimThrottleRosSteering();
                else if (_pilotMode == CarPilotMode.ROS)
                    SwitchToROSDriving();
                else
                    SwitchToManualDriving();
            }
        }

        #region Monobehavior
        private void Awake()
        {
            _carController = GetComponent<CarController>();
            _rosSim = GetComponent<RosSim>();
            _waypointFollower = GetComponent<WaypointFollower>();
            _roadBuilder = FindObjectOfType<RoadBuilder>();

            // setup steering
            _carInputs = new CarInputs(_carController);
        }

        private void FixedUpdate()
        {
            if (PilotMode == CarPilotMode.ROS)
            {
                _carController.Move(
                    _rosSim.steering_angle / _carController.MaximumSteerAngle,
                    _rosSim.throttle / _carController.MaxThrottleTorque,
                    _rosSim.brake / _carController.MaxBrakeTorque,
                    0f);
            }
            else if (PilotMode == CarPilotMode.Manual)
            {
                _carInputs.UpdateValues();
                _carController.Move(
                    _carInputs.SteeringAngle,
                    _carInputs.Throttle,
                    _carInputs.Brake * _carController.MaxBrakeTorque,
                    0f);
            }
            else if (PilotMode == CarPilotMode.SimSteeringRosThrottle)
            {
                _waypointFollower.applyThrottle = false;
                _carController.ApplyDrive(
                    _rosSim.throttle / _carController.MaxThrottleTorque,
                    _rosSim.brake / _carController.MaxBrakeTorque);
            }
            else if (PilotMode == CarPilotMode.SimThrottleRosSteering)
            {
                _carController.ApplySteeringAngle(_rosSim.steering_angle / _carController.MaximumSteerAngle);
                _carController.ApplyDrive(1.0f, 0f);
            }
        }
        #endregion

        public void SwitchToManualDriving()
        {
            PilotMode = CarPilotMode.Manual;
            
            // disable simulator control
            _waypointFollower.enabled = false;
        }

        public void SwitchToSimulatedDriving()
        {
            // turn off AI script to setup waypoints
            _waypointFollower.enabled = false;

            PilotMode = CarPilotMode.Simulated;

            // set simulator controls
            _waypointFollower.enabled = true;
            _waypointFollower.applyThrottle = true;
        }

        public void SwitchToSimSteeringRosThrottle()
        {
            // turn off AI script to setup waypoints
            _waypointFollower.enabled = false;

            PilotMode = CarPilotMode.SimSteeringRosThrottle;

            // set simulator controls
            _waypointFollower.enabled = true;
            _waypointFollower.applyThrottle = false;
        }

        public void SwitchToSimThrottleRosSteering()
        {
            _waypointFollower.enabled = false;
            PilotMode = CarPilotMode.SimThrottleRosSteering;
        }

        public void ChangeLane()
        {
            if (!(PilotMode == CarPilotMode.Simulated ||
                PilotMode == CarPilotMode.SimSteeringRosThrottle ||
                PilotMode == CarPilotMode.SimThrottleRosSteering))
                return;
            
            // turn off AI script to setup waypoints
            _waypointFollower.enabled = false;

            _roadBuilder.ChangeLanes();

            // setup simulator waypoints
            _roadBuilder.GenerateWaypoints();
            _waypointFollower.enabled = true;
        }

        public void SwitchToROSDriving()
        {
            PilotMode = CarPilotMode.ROS;

            // disable simulated control
            _waypointFollower.enabled = false;
        }

        public void SetMaxSpeed(float param)
        {
            _carController.MaxSpeed = param;
        }
    }
}
