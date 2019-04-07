using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;



namespace UnityStandardAssets.Vehicles.Car
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        MPH,
        KPH
    }

    public class CarController : MonoBehaviour
    {
        [SerializeField] private CarDriveType CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] private WheelCollider[] WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] WheelMeshes = new GameObject[4];
        [SerializeField] private WheelEffects[] WheelEffects = new WheelEffects[4];
        [SerializeField] private Vector3 CentreOfMassOffset;
        [Range (0, 35)][SerializeField] public float MaximumSteerAngle = 35;
        [SerializeField] private float _fullTorqueOverAllWheels = 1440f;

        [SerializeField] private float MaxHandbrakeTorque = 720f;
        [SerializeField] private float Downforce = 100f;
        [SerializeField] private SpeedType SpeedType;
        [SerializeField] private float _maxSpeed = 25;
        [SerializeField] private float SlipLimit;
        [SerializeField] private float _maxBrakeTorque = 1440f;

        public const string CSVFileName = "driving_log.csv";
        public const string DirFrames = "IMG";

        [SerializeField] private Camera CenterCamera;
        [SerializeField] private Camera LeftCamera;
        [SerializeField] private Camera RightCamera;
        [SerializeField] private FisheyeCamera _fisheyeCamera;

        private Vector3 Prevpos, Pos;
        private float SteerAngle;
        private float OldRotation;
        private Rigidbody _rigidbody;

        private const float k_ReversingThreshold = 0.01f;
        private string saveLocation = "";
        private Queue<CarSample> carSamples;
		private int TotalSamples;
		private bool isSaving;
		private Vector3 saved_position;
		private Quaternion saved_rotation;

		private int imageNumber = 0;

        public float MaxThrottleTorque
        {
            get { return _fullTorqueOverAllWheels; }
        }

        public float MaxBrakeTorque
        {
            get { return _maxBrakeTorque; }
        }

        public bool Skidding { get; private set; }

        public float BrakeInput { get; private set; }

        private bool isRecording = false;
        public bool IsRecording {
            get
            {
                return isRecording;
            }

            set
            {
                isRecording = value;
                if(value == true)
                { 
					Debug.Log("Starting to record");
					carSamples = new Queue<CarSample>();
					StartCoroutine(Sample());             
                } 
				else
                {
                    Debug.Log("Stopping record");
                    StopCoroutine(Sample());
                    Debug.Log("Writing to disk");
					//save the cars coordinate parameters so we can reset it to this properly after capturing data
					saved_position = transform.position;
					saved_rotation = transform.rotation;
					//see how many samples we captured use this to show save percentage in UISystem script
					TotalSamples = carSamples.Count;
					isSaving = true;
					StartCoroutine(WriteSamplesToDisk());

                };
            }

        }

        public Rigidbody GetRigidBody()
        {
            return _rigidbody;
        }

		public bool checkSaveLocation()
		{
			if (saveLocation != "") 
				return true;
			else
				SimpleFileBrowser.ShowSaveDialog (OpenFolder, null, true, null, "Select Output Folder", "Select");
			return false;
		}

        public float CurrentSteerAngle {
            get { return SteerAngle; }
            set { SteerAngle = value; }
        }

        public float CurrentSpeed{ get { return _rigidbody.velocity.magnitude * 2.23693629f; } }

        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                _maxSpeed = Mathf.Clamp(value, 0.0f, 35.0f);
            }
        }

        public float Revs { get; private set; }

        public float AccelInput { get; set; }

        // Use this for initialization
        private void Start ()
        {
            _rigidbody = GetComponent<Rigidbody>();
            //WheelColliders[0].attachedRigidbody.centerOfMass = CentreOfMassOffset;
            MaxHandbrakeTorque = float.MaxValue;
        }

        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor (float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp (float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        public void Update()
        {
            if (IsRecording)
            {
                //Dump();
            }
        }

        public void Move (float steering, float accel, float footbrake, float handbrake)
        {
            for (int i = 0; i < 4; i++) {
                Quaternion quat;
                Vector3 position;
                WheelColliders[i].GetWorldPose(out position, out quat);
                WheelMeshes[i].transform.position = position;
                WheelMeshes[i].transform.rotation = quat;
            }

            // handbrake = Mathf.Clamp(handbrake, 0, 1);

            ApplySteeringAngle(steering);
            ApplyDrive(accel, footbrake);

            //Set the handbrake.
            //Assuming that wheels 2 and 3 are the rear wheels.
            /*if (handbrake > 0f)
            {
                var hbTorque = handbrake * MaxHandbrakeTorque;
                WheelColliders [2].brakeTorque = hbTorque;
                WheelColliders [3].brakeTorque = hbTorque;
            }*/

            AddDownForce();
            //CheckForWheelSpin();
        }

        private void CapSpeed()
        {
            float speed = _rigidbody.velocity.magnitude;
            switch (SpeedType) {
            case SpeedType.MPH:
                speed *= 2.23693629f;
                if (speed > MaxSpeed)
                    _rigidbody.velocity = (MaxSpeed / 2.23693629f) * _rigidbody.velocity.normalized;
                break;

            case SpeedType.KPH:
                speed *= 3.6f;
                if (speed > MaxSpeed)
                    _rigidbody.velocity = (MaxSpeed / 3.6f) * _rigidbody.velocity.normalized;
                break;
            }
        }

        public void ApplySteeringAngle(float steeringAngle)
        {
            steeringAngle = Mathf.Clamp(steeringAngle, -1, 1);
            float intendedSteerAngle = steeringAngle * MaximumSteerAngle;
            float currentSteerAngle = SteerAngle;
            SteerAngle = Mathf.MoveTowards(currentSteerAngle, intendedSteerAngle, 2.0f);


            //Assume that wheels 0 and 1 are the front wheels.
            WheelColliders[0].steerAngle = SteerAngle;
            WheelColliders[1].steerAngle = SteerAngle;
            WheelMeshes[0].transform.localRotation = WheelColliders[0].transform.localRotation;
            WheelMeshes[1].transform.localRotation = WheelColliders[1].transform.localRotation;
        }

        public void ApplyDrive (float accel, float footbrake)
        {
            //clamp input values
            AccelInput = accel = Mathf.Clamp(accel, -1, 1);
            BrakeInput = footbrake = Mathf.Clamp(footbrake, 0, 1);

            // Apply throttle
            float thrustTorque = accel * MaxThrottleTorque;
            switch (CarDriveType) {
                case CarDriveType.FourWheelDrive:
                    thrustTorque /= 4f;
                    for (int i = 0; i < 4; i++) {
                        WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque /= 2f;
                    WheelColliders[0].motorTorque = WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque /= 2f;
                    WheelColliders[2].motorTorque = WheelColliders[3].motorTorque = thrustTorque;
                    break;
            }

            // Apply brake
            float brakeTorque = MaxBrakeTorque * footbrake;
            for (int i = 0; i < 4; i++) {
                WheelColliders[i].brakeTorque = brakeTorque;
            }

            CapSpeed();
        }


        // this is used to add more grip in relation to speed
        private void AddDownForce ()
        {
            WheelColliders[0].attachedRigidbody.AddForce(
                -transform.up * Downforce * WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }

        /*
        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin ()
        {
            // loop through all wheels
            for (int i = 0; i < 4; i++) {
                WheelHit wheelHit;
                WheelColliders [i].GetGroundHit (out wheelHit);

                // is the tire slipping above the given threshhold
                if (Mathf.Abs (wheelHit.forwardSlip) >= SlipLimit || Mathf.Abs (wheelHit.sidewaysSlip) >= SlipLimit) {
                    WheelEffects [i].EmitTyreSmoke ();
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (WheelEffects [i].PlayingAudio) {
                    WheelEffects [i].StopAudio ();
                }
                // end the trail generation
                WheelEffects [i].EndSkidTrail ();
            }
        }*/


		//Changed the WriteSamplesToDisk to a IEnumerator method that plays back recording along with percent status from UISystem script 
		//instead of showing frozen screen until all data is recorded
		public IEnumerator WriteSamplesToDisk()
		{
			yield return new WaitForSeconds(0.000f); //retrieve as fast as we can but still allow communication of main thread to screen and UISystem
			if (carSamples.Count > 0) {
				//pull off a sample from the que
				CarSample sample = carSamples.Dequeue();

				//pysically moving the car to get the right camera position
				transform.position = sample.position;
				transform.rotation = sample.rotation;

				// Capture and Persist Image
                string centerPath;

                if (_fisheyeCamera != null)
                    centerPath = WriteImage(_fisheyeCamera, "right", sample.timeStamp);
                else
				    centerPath = WriteImage(CenterCamera, "center", sample.timeStamp);

				//string row = string.Format ("{0},{1},{2},{3},{4},{5},{6}\n", centerPath, leftPath, rightPath, sample.steeringAngle, sample.throttle, sample.brake, sample.speed);
				string row = string.Format ("{0},{1},{2},{3},{4}\n", centerPath, sample.steeringAngle, sample.throttle, sample.brake, sample.speed);
				File.AppendAllText (Path.Combine (saveLocation, CSVFileName), row);
			}
			if (carSamples.Count > 0) {
				//request if there are more samples to pull
				StartCoroutine(WriteSamplesToDisk()); 
			}
			else 
			{
				//all samples have been pulled
				StopCoroutine(WriteSamplesToDisk());
				isSaving = false;

				//need to reset the car back to its position before ending recording, otherwise sometimes the car ended up in strange areas
				transform.position = saved_position;
				transform.rotation = saved_rotation;
                _rigidbody.velocity = new Vector3(0f,-10f,0f);
				Move(0f, 0f, 0f, 0f);

			}
		}

		public float getSavePercent()
		{
			return (float)(TotalSamples-carSamples.Count)/TotalSamples;
		}

		public bool getSaveStatus()
		{
			return isSaving;
		}


        public IEnumerator Sample()
        {
            // Start the Coroutine to Capture Data Every Second.
            // Persist that Information to a CSV and Perist the Camera Frame
            //yield return new WaitForSeconds(0.0666666666666667f);

			yield return new WaitForSeconds (0.1f);

            if (saveLocation != "")
            {
                CarSample sample = new CarSample();

                sample.timeStamp = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_fff");
                sample.steeringAngle = SteerAngle / MaximumSteerAngle;
                sample.throttle = AccelInput;
                sample.brake = BrakeInput;
                sample.speed = CurrentSpeed;
                sample.position = transform.position;
                sample.rotation = transform.rotation;

                carSamples.Enqueue(sample);

                sample = null;
                //may or may not be needed
            }

            // Only reschedule if the button hasn't toggled
            if (IsRecording)
            {
                StartCoroutine(Sample());
            }
				
        }

        private void OpenFolder(string location)
        {
            saveLocation = location;
            Directory.CreateDirectory (Path.Combine(saveLocation, DirFrames));
        }

        private string WriteImage(Camera camera, string prepend, string timestamp)
        {
            //needed to force camera update 
            camera.Render();
            RenderTexture targetTexture = camera.targetTexture;
            RenderTexture.active = targetTexture;
            Texture2D texture2D = new Texture2D (targetTexture.width, targetTexture.height, TextureFormat.RGB24, false);
            texture2D.ReadPixels (new Rect (0, 0, targetTexture.width, targetTexture.height), 0, 0);
            texture2D.Apply ();
            byte[] image = texture2D.EncodeToJPG ();
            UnityEngine.Object.DestroyImmediate (texture2D);
            string directory = Path.Combine(saveLocation, DirFrames);
            //string path = Path.Combine(directory, prepend + "_" + timestamp + ".jpg");
			imageNumber++;
			string path = Path.Combine(directory, imageNumber + ".jpg");
            File.WriteAllBytes (path, image);
            image = null;
            return path;
        }

        private string WriteImage(FisheyeCamera camera, string prepend, string timestamp)
        {
            //needed to force camera update 
            camera.Render();
            RenderTexture targetTexture = camera.TargetTexture;
            RenderTexture.active = targetTexture;
            Texture2D texture2D = new Texture2D (targetTexture.width, targetTexture.height, TextureFormat.RGB24, false);
            texture2D.ReadPixels (new Rect (0, 0, targetTexture.width, targetTexture.height), 0, 0);
            texture2D.Apply ();
            byte[] image = texture2D.EncodeToJPG ();
            UnityEngine.Object.DestroyImmediate (texture2D);
            string directory = Path.Combine(saveLocation, DirFrames);
            //string path = Path.Combine(directory, prepend + "_" + timestamp + ".jpg");
            imageNumber++;
            string path = Path.Combine(directory, imageNumber + ".jpg");
            File.WriteAllBytes (path, image);
            image = null;
            return path;
        }
    }

    internal class CarSample
    {
        public Quaternion rotation;
        public Vector3 position;
        public float steeringAngle;
        public float throttle;
        public float brake;
        public float speed;
        public string timeStamp;
    }

}
