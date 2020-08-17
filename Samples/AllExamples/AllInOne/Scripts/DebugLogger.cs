using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using File = System.IO.File;


namespace HoloLight.STK.Examples.AllExamples
{
    /// <summary>
    /// When this script is active, it logs all the Debug.Log events into a file and saves them.
    /// This can be used for debugging process. The file is called "LogfileSTK.txt" and is saved in
    /// the LocalState folder inside the Application Folder
    /// </summary>
    public class DebugLogger : MonoBehaviour
    {
        private static DebugLogger _instance;

        public static DebugLogger Instance
        {
            get { return _instance; }
        }

        private string _logPath;
        private string _logFileName = "LogfileSTK.txt";
        // Start is called before the first frame update
        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
            _logPath = Application.persistentDataPath;
        }

        private static void LogLines(string logMessage, TextWriter w)
        {
            string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture);
            w.WriteLine($"{timestamp}\n{logMessage}");
            w.WriteLine("-------------------------------\n");
        }

        public void Log(string logContent)
        {
            string filePath = _logPath + "\\" + _logFileName;
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 52000000)
                {
                    File.Delete(filePath);
                }
            }

            using (StreamWriter w = File.AppendText(filePath))
            {
                LogLines(logContent, w);
            }
        }

        void OnEnable()
        {
            Application.logMessageReceivedThreaded += LogCallback;
        }

        void LogCallback(string condition, string stackTrace, LogType type)
        {
            string logContent = "";
            logContent += ("stackTrace: " + stackTrace + "\n");
            logContent += "Type: " + type + "\n";
            logContent += condition;
            Log(logContent);
        }

        void OnDisable()
        {
            Application.logMessageReceivedThreaded -= LogCallback;
        }
    }
}
