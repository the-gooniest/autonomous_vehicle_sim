using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class StopSignMsg : ROSBridgeMsg
        {
            public StopSignMsg() : base() {}
            public StopSignMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/StopSign";
            }

            public float Distance
            {
                get { return (float)(_data["distance"].GetReal()); }
                set { _data["distance"] = value; }
            }

            public bool Enabled
            {
                get { return _data["enabled"].GetBoolean(); }
                set { _data["enabled"] = value; }
            }
        }
    }
}