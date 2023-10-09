/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

public class ConfigLoader : MonoBehaviour
{
    // 定義配置文件的數據結構
    private MapControl scpt_MC;
    //for launch files
    [System.NonSerialized]
    public Dictionary<string, LaunchItem> LaunchItems;
    [System.NonSerialized]
    public List<LaunchItem> LaunchItemsForRun; 
    [System.NonSerialized]
    public string curSceneItem;
    [System.NonSerialized]
    public string curMapItem;
    [System.NonSerialized]
    public string robot_x;
    [System.NonSerialized]
    public string robot_y;
    [System.NonSerialized]
    public string robot_rot;

    //for paths
    [System.NonSerialized]
    public string FileDir;
    [System.NonSerialized]
    public string mapsDir;
    [System.NonSerialized]
    public string sceneDir;
    [System.NonSerialized]
    public string showMaps;
    //for found maps files 
    [System.NonSerialized]
    public List<string> mapItems;
    [System.NonSerialized]
    public List<string> sceneItems;
    //for docker 
    [System.NonSerialized]
    public bool dockerEnable;
    [System.NonSerialized]
    public string ContainerName;
    [System.NonSerialized]
    public string dockerRunCmd;
    [System.NonSerialized]
    public string dockerExecCmd;
    //for resize
    [System.NonSerialized]
    public List<WindowAlignment> alignWindows;

    [System.Serializable]
    public class LaunchItem
    {
        public bool Enabled { get; set; }
        public int Delay { get; set; }
        public string Workspace { get; set; }
        public string Command { get; set; }
    }
    [System.Serializable]
    public class DockerConfig
    {
        public bool Enabled { get; set; }
        public string ContainerName { get; set; }
        public string RunCmd { get; set; }
        public string ExecCmd { get; set; }
    }
    [System.Serializable]
    public class WindowAlignment
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    [System.Serializable]
    public class ConfigData
    {
        public string FileDir { get; set; }
        public string mapsDir { get; set; }
        public string sceneDir { get; set; }
        public DockerConfig Docker { get; set; }
        public List<WindowAlignment> AlignWindows { get; set; }
        public Dictionary<string, LaunchItem> LaunchItems { get; set; }
    }

    //start
    void Start(){
        scpt_MC = this.gameObject.GetComponent<MapControl>();
        LaunchItemsForRun = new List<LaunchItem>() ; 
        showMaps = Application.dataPath + "/config/maps/";
        LoadConfig();
        LoadMap();
        LoadScene();
    }

    // 假設您的相對路徑是 /config/config.yaml
    private string relativePath = "/config/config.yaml";

    // 在這個函數中讀取配置文件
    public void LoadConfig()
    {
        // 使用Path.Combine來獲取完整路徑
        string fullPath = Application.dataPath + relativePath;
        //Debug.Log(fullPath);

        if (File.Exists(fullPath))
        {
            // 讀取YAML文件中的配置
            string yamlContent = File.ReadAllText(fullPath);
            var deserializer = new DeserializerBuilder().Build();
            var configData = deserializer.Deserialize<ConfigData>(yamlContent);

            // 更新相應的變數
            FileDir = configData.FileDir;
            mapsDir = configData.mapsDir;
            sceneDir = configData.sceneDir;
            scpt_MC.ShowMsgLog(ContainerName);
            //docker
            dockerEnable = configData.Docker.Enabled;
            ContainerName = configData.Docker.ContainerName;
            dockerRunCmd = configData.Docker.RunCmd;
            dockerExecCmd = configData.Docker.ExecCmd;
            //for resize
            alignWindows = configData.AlignWindows;
            //for launcher
            LaunchItems = configData.LaunchItems;
            // foreach(var LaunchItem in alignWindows){
            //     Debug.Log(LaunchItem.Name);
            // }
            // Debug.Log(ContainerName);
            // Debug.Log(mapsDir);
            // Debug.Log(sceneDir);
            // Debug.Log(ContainerName);
        }
        else
        {
            Debug.LogError("Config file not found at: " + fullPath);
        }
    }
    public void LaunchUpdate(){
        LaunchItemsForRun.Clear();
        foreach(var item in LaunchItems){
            LaunchItem itm = item.Value;
            string cmd;
            if(itm.Enabled){
                cmd = itm.Command.Replace("{curMapItem}", curMapItem);   
                cmd = cmd.Replace("{curSceneItem}", curSceneItem);   
                cmd = cmd.Replace("{robot_x}", robot_x);   
                cmd = cmd.Replace("{robot_y}", robot_y);   
                cmd = cmd.Replace("{robot_rot}", robot_rot);   
                LaunchItemsForRun.Add(new LaunchItem
                    {
                        Enabled = itm.Enabled,
                        Delay = itm.Delay,
                        Workspace = itm.Workspace,
                        Command = cmd
                    });
                //Debug.Log(itm.Command+"\n"+cmd);
            }
        }
    }
    private void LoadMap(){   
        mapItems = new List<string>();
        // 檢查資料夾是否存在
        string folderPath = showMaps;
        if (Directory.Exists(folderPath))
        {
            // 獲取所有 .jpg 文件
            string[] jpgFiles = Directory.GetFiles(folderPath, "*.jpg");

            // 提取文件名
            foreach (string filePath in jpgFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                mapItems.Add(fileName);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("資料夾不存在: " + folderPath);
        }

    }
    public void LoadScene(){
        sceneItems = new List<string>();
        // 檢查資料夾是否存在
        string folderPath = FileDir+sceneDir;
        //sceneItems.Add("Empty");
        if (Directory.Exists(folderPath))
        {
            // 獲取所有 .jpg 文件
            string[] jpgFiles = Directory.GetFiles(folderPath, "*.xml");

            // 提取文件名
            foreach (string filePath in jpgFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                sceneItems.Add(fileName);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("資料夾不存在: " + folderPath);
        }

    }


}
