using System.Collections.Generic;
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

    private LogMessages messages;
    private bool isLoadingLogs = false;
    private List<LogMessage> waitingLogs;

#if UNITY_EDITOR
    [SerializeField] private bool clearLogFile = false;
#endif

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        LoadLogs();
    }

    private void Start()
    {
        Application.logMessageReceived += OnLogMessageReceive;
        Application.quitting += OnExit;
    }

    private void OnExit()
    {
        WriteLogs();
    }

    private void OnLogMessageReceive(string condition, string stackTrace, LogType type)
    {
        AddLog("An Exeption occur in runtime", type, condition, stackTrace);
    }

    private void LoadLogs()
    {
        void Callback(bool readSucess, LogMessages messages)
        {
            if(readSucess)
            {
                this.messages = messages;
            }
            else
            {
                this.messages = new LogMessages();
            }

            isLoadingLogs = false;
            for (int i = 0; i < waitingLogs.Count; i++)
            {
                AddLog(waitingLogs[i]);
            }
        }

        waitingLogs = new List<LogMessage>();
        isLoadingLogs = true;
        Save.ReadJSONDataAsync<LogMessages>(logPath, Callback).GetAwaiter();
    }

    public void AddLog(string message, params object[] values)
    {
        AddLog(new LogMessage(message, values));
    }

    private void AddLog(LogMessage log)
    {
        if(isLoadingLogs)
        {
            waitingLogs.Add(log);
        }
        else
        {
            if (!messages.Contains(log))
                messages.AddMessage(log);
        }
    }

    public void WriteLogsAsync()
    {
        Save.WriteJSONDataAsync(messages, logPath, (bool b) => { }, true).GetAwaiter();
    }

    public void WriteLogs()
    {
        Save.WriteJSONData(messages, logPath, true);
    }

    public void ClearLog()
    {
        messages = new LogMessages();
        WriteLogs();
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        if(clearLogFile)
        {
            clearLogFile = false;
            ClearLog();
        }
    }

#endif

    #endregion

    #region Struct

    [Serializable]
    private class LogMessages
    {
        public List<LogMessage> messages;

        public LogMessages()
        {
            messages = new List<LogMessage>();
        }

        public void AddMessage(LogMessage message)
        {
            messages.Add(message);
        }

        public bool Contains(LogMessage logMessage) => messages.Contains(logMessage);
    }

    [Serializable]
    private struct LogMessage
    {
        [NonSerialized] private int id;
        private string errorMessage;
        private LogParams[] logParams;

        private LogMessage(string errorMessage, LogParams[] logParams)
        {
            this.errorMessage = errorMessage;
            this.logParams = logParams;
            id = HashCode.Combine(errorMessage, logParams);
        }

        public LogMessage(string errorMessage, params object[] objs)
        {
            this.errorMessage = errorMessage;
            logParams = new LogParams[objs.Length];

            for (int i = 0; i < objs.Length; i++)
            {
                object obj = objs[i];
                string type = obj.GetType().Name;
                string value = string.Empty;
                if (obj.GetType().IsArray)
                {
                    Array arr = (Array)obj;
                    StringBuilder sb = new StringBuilder("[ ");
                    for (int l = 0; l < arr.Length; l++)
                    {
                        sb.Append(arr.GetValue(l).ToString());
                        sb.Append(", ");
                    }
                    if (arr.Length > 0)
                    {
                        sb.Remove(sb.Length - 2, 2);
                    }
                    sb.Append(" ]");
                    value = sb.ToString();
                }
                else
                {
                    value = obj.ToString();
                }
                logParams[i] = new LogParams(type, value);
            }

            id = HashCode.Combine(errorMessage, logParams);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, null) && object.ReferenceEquals(obj, null))
                return true;

            if (object.ReferenceEquals(this, null) || object.ReferenceEquals(obj, null))
                return false;

            if (obj is LogMessage logMessage)
                return this == logMessage;
            return false;
        }

        public static bool operator ==(LogMessage log1, LogMessage log2) => log1.id == log2.id;
        public static bool operator !=(LogMessage log1, LogMessage log2) => log1.id != log2.id;

        public override int GetHashCode() => id;

        public override string ToString()
        {
            return Save.ConvertObjectToJSONString(this);
        }

        [Serializable]
        private struct LogParams
        {
            public string type;
            public string value;

            public LogParams(string type, string value)
            {
                this.type = type;
                this.value = value;
            }
        }
    }

    #endregion
}
