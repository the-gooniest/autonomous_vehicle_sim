using System;
using LitJson;
using System.Collections.Generic;
using UnityEngine;

namespace ROSBridgeLib
{
    namespace std_msgs
    {
        public class Float32MultiArrayMsg : ROSBridgeMsg
        {
            public Float32MultiArrayMsg() : base() {}
            public Float32MultiArrayMsg(JsonData msg) : base(msg) {}

            public override string ROSMessageType()
            {
                return "std_msgs/Float32MultiArray";
            }

            public List<float> ArrayData
            {
                get
                {
                    var arrayData = new List<float>();
                    string data = _data["data"].GetString();
                    double[] parsedData = Array.ConvertAll(data.Split(','), Double.Parse);
                    foreach (var element in parsedData)
                        arrayData.Add((float)element);
                    return arrayData;
                }
                set
                {  
                    _data["data"] = new JsonData();
                    foreach (float f in value)
                        _data["data"].Add(f);
                    _data["layout"] = new JsonData();
                    _data["layout"]["dim"] = new JsonData();
                    _data["layout"]["dim"].Add(0);
                    _data["layout"]["dim"].Clear();
                    _data["layout"]["data_offset"] = 0;
                }
            }
        }
    }
}