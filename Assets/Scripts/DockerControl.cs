/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
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
    public Button closeButton; 
    public Button unpauseButton;
    public Button rearrangeWinButton;
    public GameObject checkboxPrefab; // 复选框的预制体
    public Transform contentPanel; // 垂直滚动视图的内容面板


    private int sessionNum =1;
    private bool alignSessionNumFlg = false;
    private int alignSessionNum;
    private bool unpauseSessionNumFlg = false;
    private int unpauseSessionNum;
    private bool unpauseFlg = false;
    private bool closeFlg = false;

    

    private string[] stringArray; // 存储字符串的数组     
    [System.NonSerialized]
    public bool isStart = false;
    private bool dockerEnable;
    private string dockerRunCmd;
    private string dockerExecCmd;

    void Start()
    {
        scpt_cfg = this.gameObject.GetComponent<ConfigLoader>();
        // 添加按鈕點擊事件
        startButton.onClick.AddListener(StartDocker);
        stopButton.onClick.AddListener(StopDocker);   
        closeButton.onClick.AddListener(CloseAllProcess);   
        unpauseButton.onClick.AddListener(UnpausePedestrian);   
        rearrangeWinButton.onClick.AddListener(RunAlign);   
        CheckBoxInit();
        dockerEnable = scpt_cfg.dockerEnable;
        dockerRunCmd = scpt_cfg.dockerRunCmd;
        dockerExecCmd = scpt_cfg.dockerExecCmd;
    }

    private void CheckBoxInit(){

        int itemCount = scpt_cfg.LaunchItems.Count;
        float newHeight = 30f * itemCount;
        // 获取contentPanel的RectTransform
        RectTransform contentPanelRectTransform = contentPanel.parent.GetComponent<RectTransform>();
        // 创建一个新的Vector2，并将其分配给sizeDelta属性
        Vector2 newSizeDelta = contentPanelRectTransform.sizeDelta;
        newSizeDelta.y = newHeight;
        contentPanelRectTransform.sizeDelta = newSizeDelta;

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
        if(!unpauseSessionNumFlg){
            unpauseSessionNum = sessionNum;
            sessionNum+=1;
        }
        if(!closeFlg && !unpauseSessionNumFlg){
            RunDockerPsCommand("tmux",$"new-session -d -s session{unpauseSessionNum}");
            RunDockerPsCommand("tmux",$"send-keys -t session{unpauseSessionNum} \" cd {scpt_cfg.FileDir}\" C-m ");
            if(dockerEnable){
                RunDockerPsCommand("tmux",$"send-keys -t session{unpauseSessionNum} \" {AccessDocker()}\" C-m ");
            }
        }
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
        if(!unpauseSessionNumFlg){unpauseSessionNumFlg = true;}

    }
    private void RunAlign(){
        if(!alignSessionNumFlg){
            alignSessionNum = sessionNum;
            sessionNum+=1;
        }
        if(!closeFlg && !alignSessionNumFlg){
            RunDockerPsCommand("tmux",$"new-session -d -s session{alignSessionNum}");
            RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" cd {scpt_cfg.FileDir}\" C-m ");
            if(dockerEnable){
                RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" {AccessDocker()}\" C-m ");
            }    
        }
        
        foreach(var item in scpt_cfg.alignWindows){ 
            RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"{item.Name}\\\" -b remove,maximized_vert,maximized_horz\" C-m    ");      
            RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \"wmctrl -r \\\"{item.Name}\\\" -e 0,{item.X},{item.Y},{item.Width},{item.Height}\" C-m                         ");  
        }
        if(!alignSessionNumFlg){alignSessionNumFlg = true;}
    }
    private void RunCmd(int delay, string ws,string arg){
        RunDockerPsCommand("sleep",$"{delay}");
        if(!closeFlg){
            RunDockerPsCommand("tmux",$"new-session -d -s session{sessionNum}");
            RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" cd {scpt_cfg.FileDir}\" C-m ");
            if(dockerEnable){
                RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" {AccessDocker()}\" C-m ");
            }
        }
        RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" source {ws}/devel/setup.bash \" C-m ");
        RunDockerPsCommand("tmux",$"send-keys -t session{sessionNum} \" {arg} \" C-m ");
        sessionNum+=1;
    }
    private void StopDocker()
    {


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
        isStart = false;
        startButton.image.color = Color.white;
        sessionNum=1;
        alignSessionNumFlg = false;
        unpauseSessionNumFlg = false;
        unpauseFlg = false;
        closeFlg = false;
    }
    private void CloseAllProcess()
    {

        if(alignSessionNumFlg){
            RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" killall gzserver gzclient\" C-m ");
            RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" pkill roslaunch\" C-m ");
            RunDockerPsCommand("tmux",$"send-keys -t session{alignSessionNum} \" pkill rosrun\" C-m ");
        }

        
        closeFlg = true;
        isStart = false;
        startButton.image.color = Color.white;
        sessionNum=1;
        alignSessionNumFlg = false;
        unpauseSessionNumFlg = false;
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
    private string AccessDocker(){
        string dockerPsOutput = RunDockerPsCommand("docker","ps");

        // 解析输出以获取CONTAINER ID和IMAGE信息
        string[] lines = dockerPsOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            if (line.Contains(scpt_cfg.ContainerName))
            {
                return dockerExecCmd;
            }
        }

        return dockerRunCmd;
 
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
