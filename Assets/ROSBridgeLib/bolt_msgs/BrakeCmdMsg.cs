using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class BrakeCmdMsg : ROSBridgeMsg
        {
            public BrakeCmdMsg() : base() {}
            public BrakeCmdMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/BrakeCmd";
            }

            public bool Enable
            {
                get { return _data["enable"].GetBoolean(); }
                set { _data["enable"] = value; }
            }

            public float BrakeCommand
            {
                get { return (float)(_data["brake_cmd"].GetReal()); }
                set { _data["brake_cmd"] = value; }
            }

            public bool BrakeOnOff
            {
                get { return _data["boo_cmd"].GetBoolean(); }
                set { _data["boo_cmd"] = value; }
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