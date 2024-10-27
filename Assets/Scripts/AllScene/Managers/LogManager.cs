using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Collections;

public class LogManager : MonoBehaviour
{
    private const string logPath = @"/Save/UserSave/Log.txt";
    private static LogManager _instance;
    public static LogManager instance
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _instance == null)
                return GameObject.FindObjectOfType<LogManager>();
            return _instance;
#else
            return _instance;
#endif
        }
        private set
        {
            _instance = value;
        }
    }

    private LogMessages messages;
    private bool isLoadingLogs = false;
    private List<LogMessage> waitingLogs;

    [SerializeField] private int maxLogs = 3000;

#if UNITY_EDITOR
    [SerializeField] private bool clearLogFile = false;
#endif

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        Application.logMessageReceived += OnLogMessageReceive;
        Application.quitting += OnExit;
        LoadLogs();
    }

    private void OnExit()
    {
        WriteLogs();
    }

    private void OnLogMessageReceive(string condition, string stackTrace, LogType type)
    {
        AddLog(new LogMessage("An Exeption occur at runtime", stackTrace, type, condition));
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
        AddLog(new LogMessage(message, GetCurrentStackTrace(), values));
    }

    private string GetCurrentStackTrace()
    {
        StackTrace current = new StackTrace(true);
        StringBuilder sb = new StringBuilder("at ");
        StackFrame[] frames = current.GetFrames();

        for (int i = 2; i < frames.Length; i++)
        {
            var method = frames[i].GetMethod();
            sb.Append(method.DeclaringType.Name);
            sb.Append(".");
            sb.Append(method.Name);
            sb.Append("() (at ");
            string fileName = frames[i].GetFileName();
            int index = fileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            string shortFileName = index <= 0 ? string.Empty : fileName.Substring(index).Replace(@"\\", @"\");
            sb.Append(shortFileName);
            sb.Append(":");
            sb.Append(frames[i].GetFileLineNumber());
            sb.Append(") ");
        }

        return sb.ToString();
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
            {
                if (messages.length >= maxLogs)
                {
                    messages.RemoveFirst();
                }
                messages.AddMessage(log);
            }
        }
    }

    public void WriteLogsAsync()
    {
        // TODO add boolean check from the WriteJSONDataAsync function return
		Save.WriteJSONDataAsync(messages, logPath, (bool b) => { }, true).GetAwaiter();
    }

    public void WriteLogs()
    {
        if (!Save.WriteJSONData(messages, logPath, withIndentation:true, mkdir:true))
			UnityEngine.Debug.LogWarning("Couldn't save logs to disk !!!");
    }

    public void ClearLog()
    {
        messages = new LogMessages();
        WriteLogs();
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceive;
        Application.quitting -= OnExit;
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
        maxLogs = Mathf.Max(0, maxLogs);
    }

#endif

    #endregion

    #region Struct

    [Serializable]
    private class LogMessages
    {
        public int length => messages.Count;
        public List<LogMessage> messages;

        public LogMessages()
        {
            messages = new List<LogMessage>();
        }

        public void AddMessage(in LogMessage message)
        {
            if(!messages.Contains(message))
            {
                messages.Add(message);
            }
        }

        public void RemoveFirst()
        {
            messages.RemoveAt(0);
        }

        public bool Contains(LogMessage logMessage) => messages.Contains(logMessage);
    }

    [Serializable]
    private struct LogMessage : IEquatable<LogMessage>
    {
        [NonSerialized] private int _id;
        private int id
        {
            get
            {
                if (_id == default(int))
                    _id = ComputeId();
                return _id;
            }
            set => _id = value;
        }

        [SerializeField] private string errorMessage;
        [SerializeField] private LogParams[] logParams;
        [SerializeField] private string stackTrace;

        public LogMessage(string errorMessage, string stackTrace, params object[] objs)
        {
            this.errorMessage = errorMessage;
            this.stackTrace = stackTrace;
            logParams = new LogParams[objs.Length];

            StringBuilder sb = new StringBuilder(); 
            for (int i = 0; i < objs.Length; i++)
            {
                object obj = objs[i];
                string type = obj.GetType().Name;
                string value = string.Empty;
                if (obj.GetType().IsArray)
                {
                    Array arr = (Array)obj;
                    sb.Clear();
                    sb.Append("[");
                    for (int l = 0; l < arr.Length; l++)
                    {
                        sb.Append(arr.GetValue(l).ToString());
                        sb.Append(", ");
                    }
                    if (arr.Length > 0)
                    {
                        sb.Remove(sb.Length - 2, 2);
                    }
                    sb.Append("]");
                    value = sb.ToString();
                }
                if(obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(List<>))
                {
                    IList lst = (IList)obj;
                    sb.Clear();
                    sb.Append("[");
                    for (int l = 0; l < lst.Count; l++)
                    {
                        sb.Append(lst[l].ToString());
                        sb.Append(", ");
                    }
                    if (lst.Count > 0)
                    {
                        sb.Remove(sb.Length - 2, 2);
                    }
                    sb.Append("]");
                    value = sb.ToString();
                }
                else if(obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    IDictionary dic = (IDictionary)obj;
                    sb.Clear();
                    sb.Append("[");
                    foreach (object key in dic.Keys)
                    {
                        sb.Append("{");
                        sb.Append(key.ToString());
                        sb.Append(":");
                        sb.Append(dic[key].ToString());
                        sb.Append("}, ");
                    }

                    if (dic.Count > 0)
                    {
                        sb.Remove(sb.Length - 2, 2);
                    }
                    sb.Append("]");
                    value = sb.ToString();
                }
                else
                {
                    value = obj.ToString();
                }

                logParams[i] = new LogParams(type, value);
            }

            _id = 0;
            ComputeId();
        }

        public bool Equals(LogMessage other)
        {
            if (object.ReferenceEquals(this, null) && object.ReferenceEquals(other, null))
                return true;

            if (object.ReferenceEquals(this, null) || object.ReferenceEquals(other, null))
                return false;

            return other.id == id;
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

        private int ComputeId()
        {
            int logParamsHashCode = -640585942;
            for (int i = 0; i < logParams.Length; i++)
            {
                logParamsHashCode = HashCode.Combine(logParamsHashCode, logParams[i]);
            }
            return HashCode.Combine(errorMessage, logParamsHashCode);
        }

        public override int GetHashCode() => id;

        public override string ToString()
        {
            return "LogMessage{id:" + id + "}";
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

            public override bool Equals(object obj)
            {
                if(object.ReferenceEquals(this, null) && object.ReferenceEquals(obj, null))
                    return true;

                if (object.ReferenceEquals(this, null) || object.ReferenceEquals(obj, null))
                    return false;

                if(obj is LogParams logParams)
                    return this == logParams;

                return false;
            }

            public static bool operator ==(LogParams log1, LogParams log2) => log1.type == log2.type && log1.value == log2.value;
            public static bool operator !=(LogParams log1, LogParams log2) => log1.type != log2.type || log1.value != log2.value;

            public override int GetHashCode()
            {
                return HashCode.Combine(type, value);
            }
        }
    }

    #endregion
}
