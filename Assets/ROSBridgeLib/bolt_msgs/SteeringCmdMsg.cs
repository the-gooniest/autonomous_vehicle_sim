using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class SteeringCmdMsg : ROSBridgeMsg
        {
            public SteeringCmdMsg() : base() {}
            public SteeringCmdMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/SteeringCmd";
            }

            public float SteeringWheelAngle
            {
                get { return (float)(_data["steering_wheel_angle_cmd"].GetReal()); }
                set { _data["steering_wheel_angle_cmd"] = value; }
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