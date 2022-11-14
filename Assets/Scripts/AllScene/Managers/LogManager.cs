using System.Collections.Generic;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager instance { get; private set; }

    private logMessages logMessages;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    [System.Serializable]
    private class logMessages
    {
        public List<LogMessage> messages;

        public logMessages()
        {
            messages = new List<LogMessage>();
        }

        public void AddMessage(LogMessage message)
        {
            messages.Add(message);
        }
    }

    [System.Serializable]
    private class LogMessage
    {
        public string message;

        public LogMessage(string message)
        {
            this.message = message;
        }
    }
}
