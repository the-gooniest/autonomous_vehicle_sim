using LitJson;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace bolt_msgs
    {
        public class TargetPathMsg : ROSBridgeMsg
        {
            public TargetPathMsg() : base() {}
            public TargetPathMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "bolt_msgs/TargetPath";
            }

            public float C1
            {
                get { return (float)(_data["c1"].GetReal()); }
                set { _data["c1"] = value; }
            }

            public float C2
            {
                get { return (float)(_data["c2"].GetReal()); }
                set { _data["c2"] = value; }
            }

            public float C3
            {
                get { return (float)(_data["c3"].GetReal()); }
                set { _data["c3"] = value; }
            }

            public float C4
            {
                get { return (float)(_data["c4"].GetReal()); }
                set { _data["c4"] = value; }
            }

            public float[] TargetPathAsArray
            {
                get
                {
                    var targetPath = new float[4];
                    targetPath[0] = C1;
                    targetPath[1] = C2;
                    targetPath[2] = C3;
                    targetPath[3] = C4;
                    return targetPath;
                }
                set
                {
                    C1 = value[0];
                    C2 = value[1];
                    C3 = value[2];
                    C4 = value[3];
                }
            }
        }
    }
}