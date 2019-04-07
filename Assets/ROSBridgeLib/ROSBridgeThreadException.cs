using System;

public class ROSBridgeThreadException: Exception
{
    public ROSBridgeThreadException()
    {
    }

    public ROSBridgeThreadException(string message)
        : base(message)
    {
    }

    public ROSBridgeThreadException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

