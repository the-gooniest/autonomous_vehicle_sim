using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

[RequireComponent(typeof(RosSim))]
public class PISpeedController : MonoBehaviour {

    private Rigidbody _rigidbody;
    private CarController _carController;
    private RosSim _rosLateralSim;

    /// <summary>
    /// The target velocity of the vehicle in meters/sec
    /// </summary>
    private float targetVelocity = 2.2352f; // 5 mph

    private Queue<float> _pastVelocities;
    private Queue<float> PastVelocities
    {
        get
        {
            if (_pastVelocities == null)
                _pastVelocities = new Queue<float>();
            return _pastVelocities;
        }
    }

	// Use this for initialization
	void Start () {
        _rigidbody = GetComponent<Rigidbody>();
        _carController = GetComponent<CarController>();
        _rosLateralSim = GetComponent<RosSim>();
        StartCoroutine(UpdateIntegralVelocities());
        StartCoroutine(MoveCar());
	}
	
    /// <summary>
    /// Move the car according to the aquired steering angle
    /// </summary>
    private IEnumerator MoveCar()
    {
        // Wait for lateral sim to establish communication with the ROS lateral controller
        while (!_rosLateralSim.Ready)
            yield return null;

        while (true)
        {
            // Calculate new acceleration
            float velocityDiff = targetVelocity - _rigidbody.velocity.magnitude;
            float integralTerm = GetIntegralTerm();
            float newAcceleration = Mathf.Clamp(velocityDiff + integralTerm * 2.0f, -1, 1);

            // Convert _rosLateralSim.steering_angle from radians to a proportion (clamped -1 to 1)
            // of the maximum angle the car can turn.
            float convertedSteeringAngle = Mathf.Clamp((-_rosLateralSim.steering_angle * Mathf.Rad2Deg) / _carController.MaximumSteerAngle, -1, 1);

            // Move the car
            if (newAcceleration >= 0)
                _carController.Move(convertedSteeringAngle, newAcceleration, 0, 0);
            else
                _carController.Move(convertedSteeringAngle, 0, -newAcceleration, 0);

            yield return new WaitForFixedUpdate();
        }
    }

    #region Coroutines
    private IEnumerator UpdateIntegralVelocities()
    {
        while (true)
        {
            float velocityDiff = targetVelocity - _rigidbody.velocity.magnitude;
            PastVelocities.Enqueue(velocityDiff);
            if (PastVelocities.Count > 10)
                PastVelocities.Dequeue();
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    float GetIntegralTerm()
    {
        float integralTerm = 0.0f;
        foreach (float velocity in PastVelocities)
            integralTerm += velocity;
        if (integralTerm != 0.0f)
            integralTerm /= PastVelocities.Count;
        return integralTerm;
    }
}
