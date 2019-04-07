using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class ThrottleCmdMsg : ROSBridgeMsg
        {
            public ThrottleCmdMsg() : base() {}
            public ThrottleCmdMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/ThrottleCmd";
            }

            public bool Enable
            {
                get { return _data["enable"].GetBoolean(); }
                set { _data["enable"] = value; }
            }

            public float ThrottleCommand
            {
                get { return (float)(_data["throttle_cmd"].GetReal()); }
                set { _data["throttle_cmd"] = value; }
            }

            public bool Clear
            {
                get { return _data["clear"].GetBoolean(); }
                set { _data["clear"] = value; }
            }

            public bool Ignore
            {
                get { return _data["ignore"].GetBoolean(); }
                set { _data["ignore"] = value; }
            }
        }
    }
}