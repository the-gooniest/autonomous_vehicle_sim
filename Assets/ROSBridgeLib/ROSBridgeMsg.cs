using System.Collections;
using System.Text;
using UnityEngine;
using LitJson;

public abstract class ROSBridgeMsg {

    protected JsonData _data;

    public ROSBridgeMsg()
    {
        _data = new JsonData();
    }

    public ROSBridgeMsg(JsonData data)
    {
        _data = data;
    }

    public string ToJson()
    {
        return _data.ToJson();
    }

    public abstract string ROSMessageType();

    public virtual void Callback() { }
}
