using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class SteeringReportMsg : ROSBridgeMsg
        {
            public SteeringReportMsg() : base() {}
            public SteeringReportMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/SteeringReport";
            }

            public float SteeringWheelAngle
            {
                get { return (float)(_data["steering_wheel_angle"].GetReal()); }
                set { _data["steering_wheel_angle"] = value; }
            }

            public float SteeringWheelTorque
            {
                get { return (float)(_data["steering_wheel_torque"].GetReal()); }
                set { _data["steering_wheel_torque"] = value; }
            }

            public bool Enabled
            {
                get { return _data["enabled"].GetBoolean(); }
                set { _data["enabled"] = value; }
            }

            public bool Override
            {
                get { return _data["override"].GetBoolean(); }
                set { _data["override"] = value; }
            }

            public bool Timeout
            {
                get { return _data["timeout"].GetBoolean(); }
                set { _data["timeout"] = value; }
            }
        }
    }
}