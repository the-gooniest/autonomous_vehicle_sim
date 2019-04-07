using LitJson;
using UnityEngine;

/// <summary>
/// Converts a std_msgs/String ROS message that contains a base64 string into a btye array
/// </summary>
namespace ROSBridgeLib {
    namespace unity_msgs {
        public class BinaryBase64Msg : ROSBridgeMsg
        {
            public BinaryBase64Msg() : base() {}
            public BinaryBase64Msg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "std_msgs/String";
            }

            public byte[] ByteData
            {
                get
                {
                    string data = _data["data"].GetString();
                    byte[] bytes = System.Convert.FromBase64String(data);
                    return bytes;
                }
                set
                {
                    _data["data"]  = System.Convert.ToBase64String(value);
                }
            }
        }
    }
}
