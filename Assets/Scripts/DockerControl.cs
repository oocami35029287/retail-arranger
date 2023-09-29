using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;

public class DockerControl : MonoBehaviour
{
    public Button startButton; 
    public Button stopButton; 
    public bool isStart = false;
    private string ContainerName = "oocami35029287/marslite_simulation:cuda10";
    private string FileDirectory = "/home/lab605/socially-store-robot";
    void Start()
    {
        // 添加按鈕點擊事件
        startButton.onClick.AddListener(StartDocker);
        stopButton.onClick.AddListener(StopDocker);   
  
    }

    private void StartDocker()
    {
        
        if(!isStart){
            isStart = true;
            startButton.image.color = Color.green;

            
            //RunDockerPsCommand("tmux","kill-session -a");
            //RunDockerPsCommand("sleep","3");
            string command = "source " + FileDirectory + "/auto_open.sh";

            ProcessStartInfo psi = new ProcessStartInfo("/bin/bash", $"-c \"{command}\"");
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false;

            Process process = new Process();
            process.StartInfo = psi;
            process.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log(args.Data.ToString());
            process.ErrorDataReceived += (sender, args) => UnityEngine.Debug.LogError(args.Data.ToString());

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }
    } 
    private void StopDocker()
    {
        isStart = false;
        startButton.image.color = Color.white;

        // 运行 `docker ps` 命令并捕获输出
        RunDockerPsCommand("tmux","kill-session -a");
        string dockerPsOutput = RunDockerPsCommand("docker","ps");

        // 解析输出以获取CONTAINER ID和IMAGE信息
        string[] lines = dockerPsOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.Contains(ContainerName))
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    string containerId = parts[0];
                    // UnityEngine.Debug.Log("CONTAINER ID: " + containerId);

                    string image = parts[1];
                    // UnityEngine.Debug.Log("IMAGE: " + image);
                    RunDockerPsCommand("docker", $"kill {containerId}");
            }
        }
    }

    private string RunDockerPsCommand(string cmd, string arg)
    {
        Process process = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = cmd,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = arg,
        };
        process.StartInfo = startInfo;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }
}
