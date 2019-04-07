using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;

namespace ROSBridgeLib {
    public class ROSBridgeSubscriber {

        protected string _topic;
        public delegate void SubscriberCallback(JsonData msg);
        protected List<SubscriberCallback> _callbacks = null;
        public List<SubscriberCallback> Callbacks
        {
            get { return _callbacks; }
        }

        public ROSBridgeSubscriber(string topic)
        {
            _topic = topic;
            _callbacks = new List<SubscriberCallback>();
        }

        public string Topic
        {
            get { return _topic; }
        }

        public string SubscribeMsg()
        {
            return "{\"op\":\"subscribe\",\"topic\":\"" + _topic + "\"}";
        }

        public string UnsubscribeMsg()
        {
            return "{\"op\":\"unsubscribe\",\"topic\":\"" + _topic + "\"}";
        }

        public void AddCallback(SubscriberCallback callback)
        {
            _callbacks.Add(callback);
        }
	}
}

