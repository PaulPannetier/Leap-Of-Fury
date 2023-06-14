using System.Collections.Generic;
using System.Collections;
using System.Text;
using UnityEngine;
using System;

public class LogManager : MonoBehaviour
{
    private const string logPath = "/Save/Log.txt";
    public static LogManager instance
    {
        get;
        private set;
    }

    private logMessages messages;
    private bool isWritingLogs = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        messages = new logMessages();
    }

    public void AddLog(string message, params object[] values)
    {
        LogMessage log = new LogMessage(message, values);
        messages.AddMessage(log);
    }

    public void WriteRegisterLog()
    {
        StringBuilder sb = new StringBuilder();
        foreach (LogMessage log in messages.messages)
        {
            sb.Append(log.ToString());
        }
        StartCoroutine(WriteLogAsync(sb.ToString()));
        messages.messages.Clear();
    }

    public void WriteLog(string message, params object[] values)
    {
        LogMessage log = new LogMessage(message, values);
        string str = log.ToString();
        StartCoroutine(WriteLogAsync(str));
    }

    public void ClearLog()
    {
        StartCoroutine(WriteLogAsync(""));
    }

    private IEnumerator WriteLogAsync(string logData)
    {
        while (isWritingLogs)
            yield return null;

        isWritingLogs = true;
        Save.WriteStringMultiThread(logData, logPath, (bool b) => { isWritingLogs = false; }, true);
    }

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

    private class LogMessage
    {
        public string valueMessage;
        public string errorMessage;

        public LogMessage(string errorMessage, params object[] objs)
        {
            this.errorMessage = errorMessage;

            StringBuilder sb = new StringBuilder("    Params : ");
            sb.Append(objs.Length);
            sb.Append("\n    {\n");
            foreach (object obj in objs)
            {
                sb.Append("        Type : ");
                sb.Append(obj.GetType().Name);
                sb.Append(",\n        Value : ");
                if (obj.GetType().IsArray)
                {
                    Array arr = (Array)obj;
                    StringBuilder sb2 = new StringBuilder("[ ");
                    for (int l = 0; l < arr.Length; l++)
                    {
                        sb2.Append(arr.GetValue(l).ToString());
                        sb2.Append(", ");
                    }
                    sb2.Remove(sb2.Length - 2, 2);
                    sb2.Append(" ]");
                    sb.Append(sb2.ToString());
                }
                else
                {
                    sb.Append(obj.ToString());
                }
                sb.Append("\n\n");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("    }\n");
            valueMessage = sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("LogMessage : \n{\n    Error : ");
            sb.Append(errorMessage);
            sb.Append(",\n");
            sb.Append(valueMessage);
            sb.Append("}\n");
            return sb.ToString();
        }
    }
}
