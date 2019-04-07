using ROSBridgeLib.std_msgs;
using System;
using System.Reflection;
using UnityEngine;

namespace ROSBridgeLib {
    public class ROSBridgePublisher {

        protected string _topic;
        protected ROSBridgeMsg _message;

        public ROSBridgePublisher(string topic, ROSBridgeMsg msg)
        {
            _topic = topic;
            _message = msg;
        }

        public string Topic
        {
            get { return _topic; }
        }

        public virtual string AdvertiseMsg()
        {
            return "{\"op\":\"advertise\",\"topic\":\"" + _topic + "\",\"type\":\"" + _message.ROSMessageType() + "\"}";
        }

        public string UnadvertiseMsg()
        {
            return "{\"op\":\"unadvertise\",\"topic\":\"" + _topic + "\"}";
        }

        public string JSONStringMessage(ROSBridgeMsg msg)
        {
            if (!msg.ROSMessageType().Equals(_message.ROSMessageType()))
                Debug.LogError("Wrong message type for publisher of topic " + _topic + ": expected " +
                    _message.ROSMessageType() + ", received " + msg.ROSMessageType());

            return "{\"topic\":\"" + _topic + "\",\"msg\":" + msg.ToJson() + ",\"op\":\"publish\"}";
        }
    }
}
