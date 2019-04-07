using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace dbw_mkz_msgs
    {
        public class BrakeCmdMsg : ROSBridgeMsg
        {
            public BrakeCmdMsg() : base() {}
            public BrakeCmdMsg(JsonData msg) : base(msg) {}

            public enum PedalCmdType
            {
                CMD_NONE, CMD_PEDAL, CMD_PERCENT, CMD_TORQUE
            }

            public override string ROSMessageType()
            {
                return "dbw_mkz_msgs/BrakeCmd";
            }

            public float Throttle
            {
                get { return (float)(_data["pedal_cmd"].GetReal()); }
                set { _data["pedal_cmd"] = value; }
            }

            public PedalCmdType CmdType
            {
                get { return (PedalCmdType)_data["pedal_cmd_type"].GetNatural(); }
                set { _data["pedal_cmd_type"] = (int)value; }
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

            public bool Enable
            {
                get { return _data["enable"].GetBoolean(); }
                set { _data["enable"] = value; }
            }

            public bool Ignore
            {
                get { return _data["ignore"].GetBoolean(); }
                set { _data["ignore"] = value; }
            }
        }
    }
}