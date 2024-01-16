/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.IO;
using System;
public class SceneFileGenerator : MonoBehaviour
{
    private Dictionary<int, GameObject> PedestrainIDList;
    private List<GameObject> wallLineList;
    private GameObject robot;
    public Button saveButton;
    private List<int> NumArray;

    private ConfigLoader scpt_cfg;
    private MapControl scpt_MC;
    private string folderPath;
    void Start()
    {
        scpt_cfg = this.gameObject.GetComponent<ConfigLoader>();
        scpt_MC = this.gameObject.GetComponent<MapControl>();
        folderPath = scpt_cfg.FileDir + scpt_cfg.sceneDir;
        saveButton.onClick.AddListener(SaveScene);
        // 创建XML文档

    }
    private void SaveScene(){
        wallLineList = scpt_MC.wallLineList;
        PedestrainIDList = scpt_MC.PedestrainIDList;
        robot = scpt_MC.robotIcon;
        XmlDocument xmlDoc = new XmlDocument();
        XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDeclaration);

        // 创建根节点
        XmlElement scenarioElement = xmlDoc.CreateElement("scenario");
        xmlDoc.AppendChild(scenarioElement);

        // 添加障碍物
        AddObstacles(xmlDoc, scenarioElement);

        // 添加AgentClusters
        AddAgentClusters(xmlDoc, scenarioElement);
        // 添加機器人
        AddRobot(xmlDoc, scenarioElement);
        // 保存XML文件
        xmlDoc.Save($"{folderPath}Scene{SceneNum()}.xml");
        //UnityEngine.Debug.Log($"{folderPath}Scene{SceneNum()}.xml");
        scpt_MC.RefreshSceneOption();
    }
    private int SceneNum(){
        int i = -1;
        NumArray = new List<int>();
        // 檢查資料夾是否存在
        if (Directory.Exists(folderPath))
        {
            // 獲取所有 .jpg 文件
            string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");
            Regex regex = new Regex(@"Scene(\d+)\.xml");

            // 提取文件名
            foreach (string fileName in xmlFiles)
            {
                Match match = regex.Match(fileName);
                if(match.Success){
                    //UnityEngine.Debug.Log("match");
                    string numberStr = match.Groups[1].Value;
                    if (int.TryParse(numberStr, out int sceneNumber))
                    {
                        NumArray.Add(sceneNumber);
                        //UnityEngine.Debug.Log(sceneNumber);
                    }
                }
            }
            NumArray.Sort();
            do{
                i+=1;
            }while(i<NumArray.Count && NumArray[i]==i);
            UnityEngine.Debug.Log(i);
        }
        else
        {
            UnityEngine.Debug.LogError("資料夾不存在: " + folderPath);
        }
        return i;

    }
    // <obstacle x1="2" y1="4" x2="2" y2="6"/>
    void AddObstacles(XmlDocument xmlDoc, XmlElement scenarioElement)
    {
        XmlComment comment = xmlDoc.CreateComment("Obstacles");
        scenarioElement.AppendChild(comment);

        foreach(var wall in wallLineList){
            if(wall!=null){
                WallController scpt_wall = wall.GetComponent<WallController>();
                Vector3 startPoint = scpt_wall.startPoint*scpt_MC.mapResolution;
                Vector3 endPoint = scpt_wall.endPoint*scpt_MC.mapResolution;

                // 创建障碍物元素
                XmlElement obstacleElement = xmlDoc.CreateElement("obstacle");
                obstacleElement.SetAttribute("x1", startPoint.x.ToString());
                obstacleElement.SetAttribute("y1", startPoint.y.ToString());
                obstacleElement.SetAttribute("x2", endPoint.x.ToString());
                obstacleElement.SetAttribute("y2", endPoint.y.ToString());

                // 将障碍物元素添加到XML
                scenarioElement.AppendChild(obstacleElement);    
            }
            
            
        }
    }
    void AddRobot(XmlDocument xmlDoc, XmlElement scenarioElement)
    {
        XmlComment comment = xmlDoc.CreateComment("Robot");
        scenarioElement.AppendChild(comment);
        RobotAgent scpt_robot = robot.GetComponent<RobotAgent>();
        //創建agent元素
        XmlElement agentElement = xmlDoc.CreateElement("agent");
        agentElement.SetAttribute("x", (scpt_robot.position.x*scpt_MC.mapResolution).ToString("F2"));
        agentElement.SetAttribute("y", (scpt_robot.position.y*scpt_MC.mapResolution).ToString("F2"));
        agentElement.SetAttribute("r", (scpt_robot.rotate*Mathf.Deg2Rad).ToString("F6"));
        agentElement.SetAttribute("n", "1");
        agentElement.SetAttribute("dx", "0");
        agentElement.SetAttribute("dy", "0");
        agentElement.SetAttribute("type", "2");
        // 将障碍物元素添加到XML
        scenarioElement.AppendChild(agentElement);  
            
    }
    // <agent x="6" y="-1" n="2" dx="0.4" dy="1.0" type="0">
    //     <addwaypoint id="ped_wpt_corridor1_1"/>
    //     <addwaypoint id="ped_wpt_corridor1_2"/>
    // </agent>
    // <!--AgentClusters-->
    // <waypoint id="ped_wpt_corridor1_1" x="6" y="1" r="1.0"/>

    void AddAgentClusters(XmlDocument xmlDoc, XmlElement scenarioElement)
    {
        XmlComment comment = xmlDoc.CreateComment("Pedestrians");
        scenarioElement.AppendChild(comment);
        foreach (var agent in PedestrainIDList)
        {
            if(agent.Value!=null){
                PedestrianAgent scpt_ped = agent.Value.GetComponent<PedestrianAgent>();
                //創建agent元素
                XmlElement agentElement = xmlDoc.CreateElement("agent");
                agentElement.SetAttribute("x", (scpt_ped.position.x*scpt_MC.mapResolution).ToString("F2"));
                agentElement.SetAttribute("y", (scpt_ped.position.y*scpt_MC.mapResolution).ToString("F2"));
                agentElement.SetAttribute("n", scpt_ped.agentNumber.ToString());
                string div = (1.5*(1 - Math.Exp(-0.2*scpt_ped.agentNumber))).ToString("F2");
                agentElement.SetAttribute("dx", div);
                agentElement.SetAttribute("dy", div);
                agentElement.SetAttribute("type", "0");

                XmlComment comment2 = xmlDoc.CreateComment($"Pedestrians number{scpt_ped.id}");
                scenarioElement.AppendChild(comment2);
                List<WaypointVector> waypointVectorList = scpt_ped.waypointVectorList;
                foreach (var waypoint in waypointVectorList)
                {
                    if(waypoint.obj!=null){
                        WaypointAgent scpt_wp = waypoint.obj.GetComponent<WaypointAgent>();
                        XmlElement addWaypointElement = xmlDoc.CreateElement("addwaypoint");
                        string waypointName = $"waypoint{scpt_ped.id}_{scpt_wp.id}";
                        addWaypointElement.SetAttribute("id", waypointName);
                        agentElement.AppendChild(addWaypointElement);

                        XmlElement waypointElement = xmlDoc.CreateElement("waypoint");
                        waypointElement.SetAttribute("id", waypointName);
                        waypointElement.SetAttribute("x", (scpt_wp.position.x*scpt_MC.mapResolution).ToString("F2"));
                        waypointElement.SetAttribute("y", (scpt_wp.position.y*scpt_MC.mapResolution).ToString("F2"));
                        waypointElement.SetAttribute("r", "1");
                        scenarioElement.AppendChild(waypointElement);

                        
                    }
                }

                scenarioElement.AppendChild(agentElement);
            }
            
        }
    }
}
