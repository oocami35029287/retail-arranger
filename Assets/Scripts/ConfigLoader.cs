using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

public class ConfigLoader : MonoBehaviour
{
    // 定義配置文件的數據結構
    private MapControl scpt_MC;
    //for launch files
    public Dictionary<string, LaunchItem> LaunchItems;
    public List<LaunchItem> LaunchItemsForRun; 
    public string curSceneItem = "crossing_corridor";
    public string curMapItem = "crossing_corridor";

    //for paths
    public string FileDir;
    public string mapsDir;
    public string sceneDir;
    public string ContainerName;
    //for found maps files 
    public List<string> mapItems;
    public List<string> sceneItems;

    public class LaunchItem
    {
        public bool Enabled { get; set; }
        public int Delay { get; set; }
        public string Workspace { get; set; }
        public string Command { get; set; }
    }

    public class ConfigData
    {
        public Dictionary<string, LaunchItem> LaunchItems { get; set; }
        public string FileDir { get; set; }
        public string mapsDir { get; set; }
        public string sceneDir { get; set; }
        public string ContainerName { get; set; }
    }

    //start
    void Start(){
        scpt_MC = this.gameObject.GetComponent<MapControl>();
        LaunchItemsForRun = new List<LaunchItem>() ; 
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
            LaunchItems = configData.LaunchItems;
            FileDir = configData.FileDir;
            mapsDir = configData.mapsDir;
            sceneDir = configData.sceneDir;
            ContainerName = configData.ContainerName;
            scpt_MC.ShowMsgLog(ContainerName);
            // foreach(var LaunchItem in LaunchItems){
            //     Debug.Log(LaunchItem.Value.Command);
            // }
            // Debug.Log(FileDir);
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
                LaunchItemsForRun.Add(new LaunchItem
                    {
                        Enabled = itm.Enabled,
                        Delay = itm.Delay,
                        Workspace = itm.Workspace,
                        Command = cmd
                    });
                Debug.Log(itm.Command+"\n"+cmd);
            }
        }
    }
    private void LoadMap(){   
        mapItems = new List<string>();
        // 檢查資料夾是否存在
        string folderPath = FileDir+mapsDir;
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
    private void LoadScene(){
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
