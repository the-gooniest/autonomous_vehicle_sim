using LitJson;

namespace ROSBridgeLib
{
    namespace std_msgs
    {
        public class Float32Msg : ROSBridgeMsg
        {
            public Float32Msg() : base() {}
            public Float32Msg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "std_msgs/Float32";
            }

            public float FloatData
            {
                get { return (float)(_data["data"].GetReal()); }
                set { _data["data"] = value; }
            }
        }
    }
}