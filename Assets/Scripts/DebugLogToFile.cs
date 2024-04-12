/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using System.IO;
using UnityEngine;

public class DebugLogToFile : MonoBehaviour
{
    private string logFilePath;

    void Start()
    {
        logFilePath = Application.dataPath + "/config/debug_log.txt"; // 设置日志文件路径
        ClearLogFile(); // 清除文件内容
        Application.logMessageReceived += LogMessage; // 订阅日志事件
    }

    void LogMessage(string logString, string stackTrace, LogType type)
    {
        File.AppendAllText(logFilePath, $"{type}: {logString}\n");
    }
    void ClearLogFile()
    {
        if (File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, string.Empty);
        }
    }
}
