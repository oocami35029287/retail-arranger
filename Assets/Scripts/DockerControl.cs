using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class DockerControl : MonoBehaviour
{

    private ConfigLoader scpt_cfg;

    public Button startButton; 
    public Button stopButton; 
    public Button unpauseButton;
    public Button rearrangeWinButton;

    public bool isStart = false;
    private int sessionNum =1;
    private bool setAlignSessionNum = false;
    private int alignSessionNum;
    private bool setUnpauseSessionNum = false;
    private int unpauseSessionNum;
    private bool unpauseFlg = false;
  
    public GameObject checkboxPrefab; // 复选框的预制体
    public Transform contentPanel; // 垂直滚动视图的内容面板
    private string[] stringArray; // 存储字符串的数组     

    void Start()
    {
        scpt_cfg = this.gameObject.GetComponent<ConfigLoader>();
        // 添加按鈕點擊事件
        startButton.onClick.AddListener(StartDocker);
        stopButton.onClick.AddListener(StopDocker);   
        unpauseButton.onClick.AddListener(UnpausePedestrian);   
        rearrangeWinButton.onClick.AddListener(RunAlign);   
        CheckBoxInit();
    }

    private void CheckBoxInit(){
        // 根据字符串数组的长度创建复选框
        foreach (var launchFile in scpt_cfg.LaunchItems)
        {
            // 创建复选框实例
            GameObject checkbox = Instantiate(checkboxPrefab,contentPanel);
            checkbox.transform.SetParent(contentPanel);

            // 设置复选框的标签文本
            checkbox.GetComponentInChildren<Text>().text = launchFile.Key;
            checkbox.GetComponent<Toggle>().isOn = launchFile.Value.Enabled;
            checkbox.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
            {

                    string keyToFind = checkbox.GetComponentInChildren<Text>().text;
                    ConfigLoader.LaunchItem item;
                    if (scpt_cfg.LaunchItems.TryGetValue(keyToFind, out item)){
                        if (isOn){item.Enabled = true;}
                        else{item.Enabled = false;} 
                        scpt_cfg.LaunchUpdate();
                    }
                    else {UnityEngine.Debug.LogError("cannot find this launchItem: "+keyToFind);}
            });
        }
    }
    private void StartDocker()
    {
        
        if(!isStart){
            isStart = true;
            startButton.image.color = Color.green;
            
            foreach(var item in scpt_cfg.LaunchItemsForRun){
                RunCmd( item.Delay,item.Workspace, item.Command);
            }
            UnpausePedestrian();
            RunAlign();
            //OpenByShell();
        }
    } 
    private void UnpausePedestrian(){
        string unpause = "rosservice call /pedsim_simulator/unpause_simulation \\\"{}\\\"";  
        string pause = "rosservice call /pedsim_simulator/pause_simulation \\\"{}\\\"";  
        if(!setUnpauseSessionNum){
            unpauseSessionNum = sessionNum;
            setUnpauseSessionNum = true;
            sessionNum+=1;
        }
        RunDockerPsCommand("tmux",$"new-session -d -s session{unpauseSessionNum}");
        RunDockerPsCommand("tmux",$"send-keys -t session{unpauseSessionNum} \" cd {scpt_cfg.FileDir}\" C-m ");
        RunDockerPsCommand("tmux",$"send-keys -t session{unpauseSessionNum} \" source run_docker.sh same\" C-m ");
        if(!unpauseFlg){
            RunDockerPsCommand("tmux",$"send-keys -t session{unpauseSessionNum} \" {unpause} \" C-m ");
            unpauseButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Pause\nPedestrain";
            unpauseFlg = true;
        }
        else{
            RunDockerPsCommand("tmux",$"send-keys -t session{unpauseSessionNum} \" {pause} \" C-m ");
            unpauseButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "Unpause\nPedestrain";
            unpauseFlg = false;
        }

    }
    private void RunAlign(){
        if(!setAlignSessionNum){
            alignSessionNum = sessionNum;
            setAlignSessionNum = true;
            sessionNum+=1;
        }
        RunDockerPsCommand("tmux",$"new-session -d -s session{alignSessionNum}");
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" cd {scpt_cfg.FileDir}\" C-m ");
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" source run_docker.sh same\" C-m ");

        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"Gazebo\\\" -b remove,maximized_vert,maximized_horz\" C-m       ");  
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"Gazebo\\\" -e 0,960,0,960,1080\" C-m                           ");
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"Rviz\\\" -b remove,maximized_vert,maximized_horz\" C-m         ");  
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"Rviz\\\" -e 0,0,540,960,540\" C-m                              ");  
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"Yolo demo\\\" -b remove,maximized_vert,maximized_horz\" C-m    ");      
        RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"Yolo demo\\\" -e 0,0,0,0,960,540\" C-m                         ");  

    }
    private void RunCmd(int delay, string ws,string arg){
        RunDockerPsCommand("sleep",$"{delay}");
        RunDockerPsCommand("tmux",$"new-session -d -s session{sessionNum}");
        
        RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" cd {scpt_cfg.FileDir}\" C-m ");
        string arg2;
        if (sessionNum==1){
            arg2 = "cuda10";
        }
        else arg2 = "same";
        RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" source run_docker.sh {arg2}\" C-m ");
        RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" source {ws}/devel/setup.bash \" C-m ");
        RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" {arg} \" C-m ");
        sessionNum+=1;
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
            if (line.Contains(scpt_cfg.ContainerName))
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    string containerId = parts[0];
                    // UnityEngine.Debug.Log("CONTAINER ID: " + containerId);

                    string image = parts[1];
                    // UnityEngine.Debug.Log("IMAGE: " + image);
                    RunDockerPsCommand("docker", $"kill {containerId}");
            }
        }
        sessionNum=1;
        setAlignSessionNum = false;
        setUnpauseSessionNum = false;
        unpauseFlg = false;

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
    private void OpenByShell(){

            string command = "source " + scpt_cfg.FileDir + "/auto_open.sh";

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
