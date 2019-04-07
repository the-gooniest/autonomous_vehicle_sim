using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class ThrottleReportMsg : ROSBridgeMsg
        {
            public ThrottleReportMsg() : base() {}
            public ThrottleReportMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/ThrottleReport";
            }

            public float ThrottleCommand
            {
                get { return (float)(_data["throttle_cmd"].GetReal()); }
                set { _data["throttle_cmd"] = value; }
            }

            public float ThrottleOutput
            {
                get { return (float)(_data["throttle_output"].GetReal()); }
                set { _data["throttle_output"] = value; }
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

            public bool Driver
            {
                get { return _data["driver"].GetBoolean(); }
                set { _data["driver"] = value; }
            }

            public bool Timeout
            {
                get { return _data["timeout"].GetBoolean(); }
                set { _data["timeout"] = value; }
            }
        }
    }
}