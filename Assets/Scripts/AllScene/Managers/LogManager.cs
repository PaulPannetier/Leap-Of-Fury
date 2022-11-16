using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.ComponentModel;
using System;
using static UnityEngine.Rendering.DebugUI;
using UnityEditor.VersionControl;

public class LogManager : MonoBehaviour
{
    private const string logPath = "/Save/Log.txt";

    public static LogManager instance { get; private set; }

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
    }

    private void Start()
    {
        messages = new logMessages();

        Vector2 myVector = new Vector2(-11.9532f, 3.45f);
        string stringExp = "Hello world";
        int aRandomNumber = 451;
        float myFloat = 87.224f;
        int[] anArr = new int[5] { 1, 2, 3, 4, 5 };

        //WriteLog("An error occured", myVector, myFloat, anArr);
        WriteLog("An error occured, but its just another", stringExp, aRandomNumber);
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

            StringBuilder sb = new StringBuilder("Values : \n    {\n");
            foreach (object obj in objs)
            {
                /*
                sb.Append(obj.GetType().Name + obj.ToString());

                MemberInfo[] members = obj.GetType().GetMembers();
                foreach (MemberInfo memberInfo in members)
                {
                    string propName = memberInfo.Name;
                }
                */

                sb.Append("        Type : ");
                sb.Append(obj.GetType().Name);
                sb.Append(", Name : ");
                sb.Append(nameof(obj));
                sb.Append("\n        {\n");
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
                {
                    sb.Append("            {\n                Name : ");
                    string name = descriptor.Name;//Name
                    sb.Append(name);
                    sb.Append(",\n                Value : ");
                    object value = descriptor.GetValue(obj);// Value
                    sb.Append(value);
                    sb.Append("\n            },\n");

                    var type = descriptor.PropertyType;// Type
                    Console.WriteLine($"{name}={value}={type}");
                }
                sb.Append("        }\n\n");
            }
            sb.Append("    }\n");
            valueMessage = sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("LogMessage : \n{\n    Error : ");
            sb.Append(errorMessage);
            sb.Append(",\n    ");
            sb.Append(valueMessage);
            sb.Append("\n}");
            return sb.ToString();
        }
    }
}
