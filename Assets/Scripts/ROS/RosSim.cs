using ROSBridgeLib;
using ROSBridgeLib.std_msgs;
using ROSBridgeLib.unity_msgs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;
using ROSBridgeLib.bolt_msgs;
using ROSBridgeLib.sensor_msgs;

public class RosSim : MonoBehaviour {

    private static RosSim _singleton;
    public static RosSim Singleton
    {
        get
        {
            if (_singleton == null)
                _singleton = FindObjectOfType<RosSim>();
            return _singleton;
        }
    }

    public Camera cam;
    private float _publishFrequency = 1.0f;
    public float PublishFrequency
    {
        get { return _publishFrequency; }
        set { _publishFrequency = value; }
    }
    public float PublishPeriod { get { return 1.0f / PublishFrequency; } }
    private ROSBridgeController _rosController;
    private CarController _carController;
    private Rigidbody _rigidbody;
    
    private bool _ready = false;
    public bool Ready
    {
        get { return _ready; }
        private set
        {
            if (!_ready && value)
            {
                _ready = true;
                Debug.Log("Simulation ready!");
            }
        }
    }

    private float _lastTimeStamp = 0.0f;
    private Vector3 lastVelocity = Vector3.zero;

    public float steering_angle = 0.0f;
    public float throttle = 0.0f;
    public float brake = 0.0f;
    public float heading = 0.0f;
    public Vector3 acceleration = Vector3.zero;
    private List<Vector3> nearestWaypoints = new List<Vector3>();
    private float _offsetFromCenterOfLane = 0.0f;
    private Vector3 _offsetPoint;
	
    // Main Topics
    private static string _inputTopic = ROSBridgeController.MainInputTopic;
    private static string _outputTopic = ROSBridgeController.MainOutputTopic;

    // Input Subtopics
    private string _imageInTopic = _inputTopic + "/image_in";
    private string _throttleTopic = _inputTopic + "/throttle";
    private string _brakeTopic = _inputTopic + "/brake";
    private string _steeringAngleTopic = _inputTopic + "/steering_angle";

    // Output Subtopics
    private string _cameraFrontTopic = _outputTopic + "/camera_front";
    private string _offsetTopic = _outputTopic + "/offset_from_center_of_lane";
    private string _imuTopic = _outputTopic + "/imu";
    private string _speedTopic = _outputTopic + "/speed";
    private string _waypointsTopic = _outputTopic + "/waypoints";
    private string _positionTopic = _outputTopic + "/position";
    private string _headingTopic = _outputTopic + "/heading";
    private string _angularVelocityTopic = _outputTopic + "/angular_velocity";
    private string _throttleReportTopic = _outputTopic + "/throttle_report";
    private string _brakeReportTopic = _outputTopic + "/brake_report";
    private string _steeringReportTopic = _outputTopic + "/steering_report";

    #region Monobehavior
    void Start()
    {            
        if (cam == null)
        {
            Debug.LogError("Please set the cam object to one of the vehicle's cameras in the inspector.");
            return;
    	}
    				
        // Cache Components
        _rigidbody = GetComponent<Rigidbody>();
        _carController = GetComponent<CarController>();
        _rosController = ROSBridgeController.Singleton;

        // Add publishers
        _rosController.AddPublisher(_cameraFrontTopic, new BinaryBase64Msg());
        _rosController.AddPublisher(_waypointsTopic, new Float32MultiArrayMsg());
        _rosController.AddPublisher(_imuTopic, new ImuMsg());
        _rosController.AddPublisher(_positionTopic,  new Float32MultiArrayMsg());
        _rosController.AddPublisher(_offsetTopic, new Float32Msg());
        _rosController.AddPublisher(_speedTopic, new Float32Msg());
        _rosController.AddPublisher(_headingTopic, new Float32Msg());
        _rosController.AddPublisher(_angularVelocityTopic, new Float32Msg());
        _rosController.AddPublisher(_throttleReportTopic, new ThrottleReportMsg());
        _rosController.AddPublisher(_brakeReportTopic, new BrakeReportMsg());
        _rosController.AddPublisher(_steeringReportTopic, new SteeringReportMsg());

        // Add subscribers
        _rosController.AddSubscriber(_imageInTopic, new BinaryBase64Msg());
        _rosController.AddSubscriber(_throttleTopic, new ThrottleCmdMsg());
        _rosController.AddSubscriber(_brakeTopic, new BrakeCmdMsg());
        _rosController.AddSubscriber(_steeringAngleTopic, new SteeringCmdMsg());

        var image_sub = _rosController.GetSubscriber(_imageInTopic);
        if (image_sub != null)
        {
            ROSBridgeSubscriber.SubscriberCallback callback = (msg => {
                var image_msg = new BinaryBase64Msg(msg);
                byte[] jpeg_bytes = image_msg.ByteData;
                if (VisualizeImageInput.Singleton != null)
                {
                    if (!VisualizeImageInput.Singleton.enabled)
                        VisualizeImageInput.Singleton.enabled = true;
                    VisualizeImageInput.Singleton.ToggleValue = true;
                }
                VisualizeImageInput.Singleton.SetTextureWithImageData(jpeg_bytes);
            });
            image_sub.AddCallback(callback);
        }
        else
            Debug.Log("Couldn't find subscriber for topic " + _imageInTopic);
        
         // Setup steering subsciber callback
        var steering_sub = _rosController.GetSubscriber(_steeringAngleTopic);
        if (steering_sub != null)
        {
            ROSBridgeSubscriber.SubscriberCallback callback = (msg => {
                var steering_msg = new SteeringCmdMsg(msg);
                steering_angle = steering_msg.SteeringWheelAngle;
                //Debug.Log(steering_angle);
            });
            steering_sub.AddCallback(callback);
        }
        else
            Debug.Log("Couldn't find subscriber for topic " + _steeringAngleTopic);
        
        // Checks for a "ready" status update from ROS publishers
        var status_sub = _rosController.GetSubscriber(ROSBridgeController.StatusInTopic);
        if (status_sub != null)
        {
            ROSBridgeSubscriber.SubscriberCallback callback = (msg => {
            var status_msg = new StringMsg(msg);
                string status = status_msg.StringData;
                if (status.Contains("ready"))
                    Ready = true;
            });
            status_sub.AddCallback(callback);
        }
        else
            Debug.Log("Couldn't find subscriber for topic " + ROSBridgeController.StatusInTopic);

        // Throttle callback
        var throttleTopic = _throttleTopic;
        var throttle_sub = _rosController.GetSubscriber(_throttleTopic);
        if (throttle_sub != null)
        {
            ROSBridgeSubscriber.SubscriberCallback callback = (msg =>
            {
                var throttle_msg = new ThrottleCmdMsg(msg);
                throttle = throttle_msg.ThrottleCommand;
            });
            throttle_sub.AddCallback(callback);
        }
        else
            Debug.Log("Could not find subscriber for topic" + throttleTopic);

        // Start Publishers
        StartCoroutine(Publishers());
	}

    void FixedUpdate()
    {
        acceleration = (_rigidbody.velocity - lastVelocity) / Time.fixedTime;
        lastVelocity = _rigidbody.velocity;
    }

    void OnDrawGizmos()
    {
        float v = 0.0f;
        foreach (var point in nearestWaypoints)
        {
            v += 0.125f;
            float angle = Mathf.Abs(steering_angle);
            if (angle < 20)
                angle = 0;
            float e = angle / 34;
            Gizmos.color = new Color(v, v - e, v - e);
            Gizmos.DrawSphere(point, 0.5f);
        }

    }
	#endregion
    
    #region Coroutines
    private IEnumerator Publishers()
    {
        // Wait for connection to ROS
        while (_rosController == null || !_rosController.IsConnectionReady)
            yield return null;
        
        while (true)
        {
            yield return null;
            if (Time.unscaledTime - _lastTimeStamp < PublishPeriod)
                continue;

            _lastTimeStamp = Time.unscaledTime;

        	// Publish image data
            if (VisualizeImageInput.Singleton.ToggleValue)
            {
                var msg = new BinaryBase64Msg();
                byte[] byteData = GetImageData();
                msg.ByteData = byteData;
                _rosController.Publish(_cameraFrontTopic, msg);
            }

            // Publish Imu values
            var imuMsg = new ImuMsg();
            imuMsg.QuaternionData = transform.rotation;
            imuMsg.AngularVelocityData = _rigidbody.angularVelocity;
            imuMsg.LinearAccelerationData = acceleration;
            _rosController.Publish(_imuTopic, imuMsg);

            // Publish waypoints
            var wayPointsMsg = new Float32MultiArrayMsg();
            wayPointsMsg.ArrayData = WayPointsArray();
            _rosController.Publish(_waypointsTopic, wayPointsMsg);

            // Publish distance from center of the lane
            var offsetMsg = new Float32Msg();
            offsetMsg.FloatData = _offsetFromCenterOfLane;
            _rosController.Publish(_offsetTopic, offsetMsg);

            // Publish position
            var positionMsg = new Float32MultiArrayMsg();
            positionMsg.ArrayData = PositionArray();
            _rosController.Publish(_positionTopic, positionMsg);

            // Publish heading
            var forwardDirection = transform.forward;
            Vector2 fromVector2 = new Vector2(forwardDirection.x, forwardDirection.z);
            Vector2 toVector2 = new Vector2(1, 0);
            heading = Vector2.Angle(fromVector2, toVector2);
            Vector3 cross = Vector3.Cross(fromVector2, toVector2);
            if (cross.z > 0)
                heading = 360 - heading;
            heading *= Mathf.Deg2Rad;
            var headingMsg = new Float32Msg();
            headingMsg.FloatData = heading;
            _rosController.Publish(_headingTopic, headingMsg);

            // Publish speed
            var speedMsg = new Float32Msg();
            speedMsg.FloatData = _rigidbody.velocity.magnitude;
            _rosController.Publish(_speedTopic, speedMsg);

            // Publish angular velocity
            var angularVelocityMsg = new Float32Msg();
            angularVelocityMsg.FloatData = -_rigidbody.angularVelocity.y * Mathf.Deg2Rad;
            _rosController.Publish(_angularVelocityTopic, angularVelocityMsg);

            // Publish throttle values
            var throttleReportMsg = new ThrottleReportMsg();
            throttleReportMsg.ThrottleCommand = throttle;
            throttleReportMsg.ThrottleOutput = _carController.AccelInput;
            _rosController.Publish(_throttleReportTopic, throttleReportMsg);

            // Publish brake values
            var brakeReportMsg = new BrakeReportMsg();
            brakeReportMsg.BrakeCommand = brake;
            brakeReportMsg.BrakeOutput = _carController.BrakeInput;
            _rosController.Publish(_brakeReportTopic, brakeReportMsg);

            // Publish brake values
            var steeringReportMsg = new SteeringReportMsg();
            steeringReportMsg.SteeringWheelAngle = _carController.CurrentSteerAngle;
            steeringReportMsg.SteeringWheelTorque = 0;
            _rosController.Publish(_steeringReportTopic, steeringReportMsg);
        }
    }
	#endregion

    byte[] GetImageData()
    {
        RenderTexture targetTexture = cam.targetTexture;
        RenderTexture.active = targetTexture;
        Texture2D texture2D = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
        texture2D.Apply();
        byte[] byte_array = texture2D.EncodeToJPG();

        // Required to prevent leaking the texture
        DestroyImmediate(texture2D);
        return byte_array;
    }

    private List<float> WayPointsArray()
    {
        var points = new List<float>();
        var wayPoints = RoadBuilder.Singleton.Waypoints;
        nearestWaypoints = new List<Vector3>();

        // Find nearest waypoint
        int nearestWaypointIndex = 0;
        float nearestWaypointDist = Mathf.Infinity;
        Vector3 forward = transform.forward;
        for (int i = 0; i < wayPoints.Count; i++)
        {
            var waypoint = wayPoints[i];
            Vector3 direction = waypoint.transform.position - _rigidbody.position;
            float dist = direction.sqrMagnitude;
            if (dist < nearestWaypointDist)
            {
                nearestWaypointIndex = i;
                nearestWaypointDist = dist;
            }
        }

        int numWaypoints = wayPoints.Count;
        for (int i=0; i < 8; i++)
            nearestWaypoints.Add(wayPoints[(i + nearestWaypointIndex) % numWaypoints].transform.position);

        foreach (Vector3 point in nearestWaypoints)
        {
            points.Add(point.x);
            points.Add(point.z);
        }

        // Get offset from center of lane
        /*
        Vector3 leftPoint = wayPoints[(nearestWaypointIndex + 1) % numWaypoints].transform.position;
        int rightIndex = (((nearestWaypointIndex - 1) % numWaypoints) + numWaypoints)  % numWaypoints;
        Vector3 rightPoint = wayPoints[rightIndex].transform.position;
        _offsetFromCenterOfLane = OffsetFromCenterOfLane(
            leftPoint,
            rightPoint);*/

        return points;
    }

    private List<float> PositionArray()
    {
        return new List<float>() { transform.position.x, transform.position.z };
    }

    private float OffsetFromCenterOfLane(Vector3 waypoint1, Vector3 waypoint2)
    {
        float accuracy = 0.001f;
        Vector3 point1 = waypoint1;
        Vector3 point2 = waypoint2;
        float distance = (waypoint1 - waypoint2).sqrMagnitude;
        while (distance > accuracy)
        {
            float dist1 = (_rigidbody.position - point1).sqrMagnitude;
            float dist2 = (_rigidbody.position - point2).sqrMagnitude;
            if (dist1 < dist2)
                point2 = Vector3.Lerp(point2, point1, 0.5f);
            else
                point1 = Vector3.Lerp(point1, point2, 0.5f);
            distance *= 0.5f;
        }
        _offsetPoint = (point1 + point2) * 0.5f;
        return (_offsetPoint - _rigidbody.position).magnitude;
    }
}
