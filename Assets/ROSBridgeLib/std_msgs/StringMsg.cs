using LitJson;

namespace ROSBridgeLib {
	namespace std_msgs {
		public class StringMsg : ROSBridgeMsg
        {
            public StringMsg() : base() {}
            public StringMsg(string msg) {
                StringData = msg;
            }
            public StringMsg(JsonData msg) : base(msg) {}

			public override string ROSMessageType()
            {
				return "std_msgs/String";
			}

            public string StringData
            {
                get { return _data["data"].GetString(); }
                set
                {
                    _data = new JsonData();
                    _data["data"] = value;
                }
            }
        }
	}
}