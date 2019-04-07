using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace sensor_msgs
    {
        public class ImuMsg : ROSBridgeMsg
        {
            public ImuMsg() : base() {}
            public ImuMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "sensor_msgs/Imu";
            }

            public Quaternion QuaternionData
            {
                get
                {
                    return new Quaternion(
                        (float)(_data["orientation"]["x"].GetReal()),
                        (float)(_data["orientation"]["y"].GetReal()),
                        (float)(_data["orientation"]["z"].GetReal()),
                        (float)(_data["orientation"]["w"].GetReal()));
                }
                set
                {
                    _data["orientation"] = new JsonData();
                    _data["orientation"]["x"] = value.x;
                    _data["orientation"]["y"] = value.y;
                    _data["orientation"]["z"] = value.z;
                    _data["orientation"]["w"] = value.w;
                }
            }

            public Vector3 AngularVelocityData
            {
                get
                {
                    return new Vector3(
                        (float)(_data["angular_velocity"]["x"].GetReal()),
                        (float)(_data["angular_velocity"]["y"].GetReal()),
                        (float)(_data["angular_velocity"]["w"].GetReal()));
                }
                set
                {
                    _data["angular_velocity"] = new JsonData();
                    _data["angular_velocity"]["x"] = value.x;
                    _data["angular_velocity"]["y"] = value.y;
                    _data["angular_velocity"]["z"] = value.z;
                }
            }

            public Vector3 LinearAccelerationData
            {
                get
                {
                    return new Vector3(
                        (float)(_data["linear_acceleration"]["x"].GetReal()),
                        (float)(_data["linear_acceleration"]["y"].GetReal()),
                        (float)(_data["linear_acceleration"]["w"].GetReal()));
                }
                set
                {
                    _data["linear_acceleration"] = new JsonData();
                    _data["linear_acceleration"]["x"] = value.x;
                    _data["linear_acceleration"]["y"] = value.y;
                    _data["linear_acceleration"]["z"] = value.z;
                }
            }
        }
    }
}