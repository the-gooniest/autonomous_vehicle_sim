using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class BrakeReportMsg : ROSBridgeMsg
        {
            public BrakeReportMsg() : base() {}
            public BrakeReportMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/BrakeReport";
            }

            public float BrakeCommand
            {
                get { return (float)(_data["brake_cmd"].GetReal()); }
                set { _data["brake_cmd"] = value; }
            }

            public float BrakeOutput
            {
                get { return (float)(_data["brake_output"].GetReal()); }
                set { _data["brake_output"] = value; }
            }

            public bool Enabled
            {
                get { return _data["enabled"].GetBoolean(); }
                set { _data["enabled"] = value; }
            }

            public bool BrakeOnOffInput
            {
                get { return _data["boo_input"].GetBoolean(); }
                set { _data["boo_input"] = value; }
            }

            public bool BrakeOnOffCommand
            {
                get { return _data["boo_cmd"].GetBoolean(); }
                set { _data["boo_cmd"] = value; }
            }

            public bool BrakeOnOffOutput
            {
                get { return _data["boo_output"].GetBoolean(); }
                set { _data["boo_output"] = value; }
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