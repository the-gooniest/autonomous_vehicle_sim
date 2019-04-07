using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    public class CarInputs
    {
        public CarController _controller;
        public float SteeringAngle { get; private set; }
        public float Throttle { get; private set; }
		public float Brake { get; private set; }
		public bool mouse_hold;
		public float mouse_start;

        public CarInputs(CarController controller)
        {
            SteeringAngle = 0f;
            Throttle = 0f;
            Brake = 0f;
			mouse_hold = false;
        }
            
        public void UpdateValues()
        {

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                Throttle = CrossPlatformInputManager.GetAxis("Vertical");
                Brake = 0;
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                Throttle = 0;
                Brake = -CrossPlatformInputManager.GetAxis("Vertical");
            }
            else
            {
                Throttle = 0;
                Brake = 0;
            }

			// Turn Left (max -1)
			if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
			{
				if (SteeringAngle > -1.0) 
					SteeringAngle -= 0.05f;
			}
			// Turn Right (max 1)
			else if (Input.GetKey (KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
			{
				if (SteeringAngle < 1.0) 
					SteeringAngle += 0.05f;
			}

			// Right Click
            else if (Input.GetMouseButton(1))
            {
				float mousePosition = Input.mousePosition.x;

				// check if its the first time pressing down on mouse button
				if (!mouse_hold)
				{
					// we are now holding down the mouse
					mouse_hold = true;
					// set the start reference position for position tracking
					mouse_start = mousePosition;
				}
			
				// This way h is [-1, -1]
				// it's quite hard to get a max or close to max
				// steering angle unless it's actually wanted.
				SteeringAngle = Mathf.Clamp ( (mousePosition - mouse_start)/(Screen.width/6), -1, 1);
            }
            else
            {
				// reset
				mouse_hold = false;
				SteeringAngle = CrossPlatformInputManager.GetAxis ("Horizontal");
            }
        }
    }
}