using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace std_msgs
    {
        public class Int32Msg : ROSBridgeMsg
        {
            public Int32Msg() : base() {}
            public Int32Msg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "std_msgs/Int32";
            }

            public int IntData
            {
                get { return (int)(_data["data"].GetNatural()); }
                set { _data["data"] = value; }
            }
        }
    }
}