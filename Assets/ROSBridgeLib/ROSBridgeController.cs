using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using WebSocketSharp;
using ROSBridgeLib;
using ROSBridgeLib.std_msgs;

public class ROSBridgeController : MonoBehaviour {

    public static ROSBridgeController _activeController;
    public static ROSBridgeController Singleton
    {
        get
        {
            if (_activeController == null)
                _activeController = FindObjectOfType<ROSBridgeController>();
            return _activeController;
        }
    }

    private List<ROSBridgeSubscriber> _subscribers;
    private List<ROSBridgeSubscriber> Subscribers
    {
        get
        {
            if (_subscribers == null)
                _subscribers = new List<ROSBridgeSubscriber>();
            return _subscribers;
        }
    }

    private List<ROSBridgePublisher> _publishers;
    private List<ROSBridgePublisher> Publishers
    {
        get
        {
            if (_publishers == null)
                _publishers = new List<ROSBridgePublisher>();
            return _publishers;
        }
    }

    public class CallbackElement
    {
        public ROSBridgeSubscriber.SubscriberCallback _callback;
        public JsonData _data;
        public CallbackElement(ROSBridgeSubscriber.SubscriberCallback callback, JsonData data)
        {
            _callback = callback;
            _data = data;
        }
        public void Execute()
        {
            _callback(_data);
        }
    }

    public class QueuedMessage
    {
        public ROSBridgeMsg message;
        public string topic;
        public QueuedMessage(string topic, ROSBridgeMsg message)
        {
            this.topic = topic;
            this.message = message;
        }
    }

    public static string MainInputTopic
    {
        get { return "/autodrive_sim/input"; }
    }

    public static string MainOutputTopic
    {
        get { return "/autodrive_sim/output"; }
    }

    public static string StatusInTopic
    {
        get { return MainInputTopic + "/status"; }
    }

    public static string StatusOutTopic
    {
        get { return MainOutputTopic + "/status"; }
    }

    private bool _connectionThreadSuccessful = false;
    private bool _isConnectionReady = false;
    public bool IsConnectionReady
    {
        get { return _isConnectionReady && _bridgeSocket != null; }
        private set
        {
            _isConnectionReady = value;
        }
    }

    private Exception _threadException = null;
    private Queue<CallbackElement> _subscriberCallbacks = new Queue<CallbackElement>();
    private string host_ip_addr = "192.168.193.3";
    private int port = 9090;
    private string image_data;
    private WebSocket _bridgeSocket;
 	private System.Threading.Thread _connectionThread = null;
    private System.Threading.Thread _publishThread = null;
    //private Queue<QueuedMessage> _messageQueue1 = new Queue<QueuedMessage>();
    //private Queue<QueuedMessage> _messageQueue2 = new Queue<QueuedMessage>();
    //private bool availableQueue = true;
    //private object queueSwitchLock = new object();
    private RingBuffer<QueuedMessage> _messageQueue = new RingBuffer<QueuedMessage>(100);

    /*
    private Queue<QueuedMessage> AvailableQueue
    {
        get { return availableQueue ? _messageQueue1 : _messageQueue2; }
    }

    private Queue<QueuedMessage> SwitchQueues()
    {
        var lastQueue = AvailableQueue;
        lock (queueSwitchLock)
        {
            availableQueue = !availableQueue; 
        }
        return lastQueue;
    }*/

    #region Monobehaviour
    void Awake()
    {
        Application.runInBackground = true;
        var initialIpAddress = PlayerPrefs.GetString(SimulationSetup.RosBridgeIpAddressKey);
        if (initialIpAddress != null && !initialIpAddress.Equals(""))
            host_ip_addr = initialIpAddress;

        var initialPort = PlayerPrefs.GetInt(SimulationSetup.RosBridgePortKey, -1);
        if (initialPort >= 0)
            port = initialPort;

        AddPublisher(StatusOutTopic, new StringMsg());
        AddSubscriber(StatusInTopic, new StringMsg());

        // Stops the simulation if the a connecting ROS node send a "stop" status message
        var status_sub = GetSubscriber(StatusInTopic);
        if (status_sub != null)
        {
            ROSBridgeSubscriber.SubscriberCallback callback = (msg => {
                var status_msg = new StringMsg(msg);
                string status = status_msg.StringData;
                if (status.Contains("stop"))
                    SimulationSetup.LoadSimluationSetupScene();
            });
            status_sub.AddCallback(callback);
        }
        else
            Debug.Log("Couldn't find subscriber for topic " + status_sub);
    }

    void Start()
    {
        StartCoroutine(CheckForExceptionInConnectionThread());
        StartCoroutine(Connect());
    }

    private void Update()
    {
        while (_subscriberCallbacks.Count > 0)
        {
            var callback = _subscriberCallbacks.Dequeue();
            callback.Execute();
        }
    }

    void OnApplicationQuit()
    {
        // Extremely important to disconnect from ROS. Otherwise packets continue to flow
        Disconnect();
    }

    void OnDestroy()
    {
        // Extremely important to disconnect from ROS. Otherwise packets continue to flow
        Disconnect();
    }
    #endregion

    /// <summary>
    /// Expresses any exceptions that occur in the connection thread in the main update thread
    /// </summary>
    private IEnumerator CheckForExceptionInConnectionThread()
    {
        while (true)
        {
            if (_threadException == null)
            {
                yield return null;
                continue;
            }
            try
            {
                throw _threadException;
            }
            catch (ThreadAbortException)
            {
                Debug.Log("Exited RosBridge connection thread");
            }
            catch (ROSBridgeThreadException exception)
            {
                Debug.Log("ROSBridgeThreadException: " + exception.Message);
            }
            _threadException = null;
        }
    }

    /// <summary>
    /// Connect to Rosbridge
    /// </summary>
    private IEnumerator Connect() {
        _connectionThread = new System.Threading.Thread(ConnectToRosBridge);
        _connectionThread.Start();
        while (!_connectionThreadSuccessful && _bridgeSocket == null)
            yield return null;

        // Log connection
        Debug.Log("Connected to RosBridge at address " + host_ip_addr + ":" + port);

        // Advertise to topics from the added publisher objects
        foreach (ROSBridgePublisher pub in Publishers)
            AdvertisePublisher(pub);

        yield return new WaitForSecondsRealtime(1.0f);

        // Subscribe to topics from subscriber objects
        foreach (ROSBridgeSubscriber sub in Subscribers)
            SubscribeSubscriber(sub);

        // Notify publishers that the connection is ready
        IsConnectionReady = true;

        // Start publisher thread
        _publishThread = new System.Threading.Thread(PublishThread);
        _publishThread.Start();
	}

    /// <summary>
    /// Disconnect from RosBridge
    /// </summary>
	public void Disconnect() {
        // Mark connection as dead
        IsConnectionReady = false;

        // Kill threads
        if (_connectionThread != null)
            _connectionThread.Abort();
        if (_publishThread != null)
            _publishThread.Abort();

        // Check socket
        if (_bridgeSocket == null)
            return;

        // Send stop status to listening nodes
        Publish(StatusOutTopic, new StringMsg("stop")); 

        // Unsubscribe from the subscriber objects' topics
        foreach (ROSBridgeSubscriber sub in Subscribers) {
            string msg = sub.UnsubscribeMsg();
            _bridgeSocket.Send(msg);
		}

        // Unadvertise from the publisher objects' topics
		foreach(ROSBridgePublisher pub in Publishers) {
            string msg = pub.UnadvertiseMsg();
            _bridgeSocket.Send(msg);
		}

        _bridgeSocket.Close();
        Debug.Log("Closed RosBridge connection");
	}

    private void OnWebsocketConnected() {
        // Alert publishing functions that the connection is ready
        if (_bridgeSocket != null)
            _connectionThreadSuccessful = true;
    }

    #region Thread Functions
    /// <summary>
    /// Thread function that attempts to establish a connection with a ROSBridge server
    /// </summary>
	private void ConnectToRosBridge() {
        try
        {
            // Connect to RosBridge server
            _bridgeSocket = new WebSocket("ws://" + host_ip_addr + ":" + port);
            _bridgeSocket.OnOpen += (sender, e) => OnWebsocketConnected();
            _bridgeSocket.OnMessage += (sender, e) => MessageCallback(e.Data);
            _bridgeSocket.ConnectAsync();

            // Wait 5 seconds 
            for (int i=0; i<5000; i++)
                Thread.Sleep(1);
            
            if (!_connectionThreadSuccessful)
                throw new ROSBridgeThreadException("Socket timeout - could not connect to RosBridge.");

            // Wait while callbacks are being served
            while (true)
                Thread.Sleep(1000);
        }
        catch (Exception exception)
        {
            _threadException = exception;
        }
	}

    private void PublishThread()
    {
        try
        {
            while (IsConnectionReady) {
                QueuedMessage message = null;
                bool gotMessage = _messageQueue.TryDequeue(out message);
                if (gotMessage)
                    PublishMessage(message);
                Thread.Sleep(2);
            }
            throw new ROSBridgeThreadException("Exited from publish thread");
        }
        catch (Exception exception)
        {
            _threadException = exception;
        }
    }

    #endregion

    /// <summary>
    /// Callback method that processes every message received through _bridgeSocket
    /// </summary>
    private void MessageCallback(string string_msg) {
        if ((string_msg == null) || string_msg.Equals(""))
            Debug.Log("Got an empty message from the web socket");

        // Get operation string from the ROS Bridge JSON message
        JsonData parsed_message = JsonMapper.ToObject(string_msg);
        string operation = parsed_message["op"].GetString();
        if (operation.Equals("publish"))
        {
            string topic = parsed_message["topic"].GetString();
            foreach (ROSBridgeSubscriber sub in Subscribers)
            {
                if (topic.Equals(sub.Topic) && sub.Callbacks.Count > 0)
                {
                    foreach (var callback in sub.Callbacks)
                        _subscriberCallbacks.Enqueue(new CallbackElement(callback, parsed_message["msg"]));
                }
            }
        }
        else if (operation.Equals("status"))
            Debug.Log("ROSBridge status message: " + parsed_message["msg"].GetString());
        else
            Debug.Log("Message with unknown ROSBridge operation \"" + operation + "\" received.");
    }

    /// <summary>
    /// Attempts to return the subscriber object that matches the given topic from
    /// the list of already subscribed topics.
    /// </summary>
    public ROSBridgeSubscriber GetSubscriber(string topic)
    {
        return Subscribers.Find(sub => sub.Topic == topic);
    }

    public ROSBridgePublisher GetPublisher(string topic)
    {
        return Publishers.Find(pub => pub.Topic == topic);
    }

	public void Publish(string topic, ROSBridgeMsg message) {
        if (!IsConnectionReady)
            return;
        _messageQueue.Enqueue(new QueuedMessage(topic, message));
	}

    private void PublishMessage(QueuedMessage queuedMessage)
    {
        if (!IsConnectionReady)
            return;
        ROSBridgePublisher pub = _publishers.Find(x => queuedMessage.topic.Equals(x.Topic));
        if (pub == null)
            Debug.LogError("No publisher established for the topic \"" + queuedMessage.topic + "\"");

        string jsonString = pub.JSONStringMessage(queuedMessage.message);

        _bridgeSocket.Send(jsonString);
    }

    public void AddPublisher(string topic, ROSBridgeMsg msg)
    {
        var pub = new ROSBridgePublisher(topic, msg);
        Publishers.Add(pub);
        AdvertisePublisher(pub);
    }

    public void AddSubscriber(string topic, ROSBridgeMsg msg)
    {
        var sub = new ROSBridgeSubscriber(topic);
        if (GetPublisher(topic) == null)
            AddPublisher(topic, msg);
        Subscribers.Add(sub);
        SubscribeSubscriber(sub);
    }

    public void AdvertisePublisher(ROSBridgePublisher pub)
    {
        if (_bridgeSocket == null)
            return;
        string msg = pub.AdvertiseMsg();
        _bridgeSocket.Send(msg);
        //Debug.Log(msg);
    }

    public void SubscribeSubscriber(ROSBridgeSubscriber sub)
    {
        if (_bridgeSocket == null)
            return;
        string msg = sub.SubscribeMsg();
        _bridgeSocket.Send(msg);
    }
}
    