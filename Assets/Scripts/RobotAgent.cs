/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class RobotAgent : MonoBehaviour
{
    private int id = -10;
    private GameObject controller;
    private EditModeSelector scpt_EM;
    private MapControl scpt_MC;
    private Button robotButton;
    private RawImage circle;
    [System.NonSerialized]
    public Vector2 position;
    public float rotate; //degree
    private bool selected = false;
    private Color32 selectedColor;
    private Color32 unselectColor;
    private int waypointID;
    private ConfigLoader scpt_cfg;
    // public AgentVector agentVector;
    public List<WaypointVector> waypointVectorList;
    //建構函數
    public void Initialize()
    {
        
        
        scpt_EM = GameObject.Find("Controller").GetComponent<EditModeSelector>();
        scpt_MC = GameObject.Find("Controller").GetComponent<MapControl>();
        scpt_cfg = GameObject.Find("Controller").GetComponent<ConfigLoader>();
        //位置
        this.position = new Vector2(0,0);
        this.rotate = 0;
        scpt_cfg.robot_x = (position.x*scpt_MC.mapResolution).ToString();
        scpt_cfg.robot_y = (position.y*scpt_MC.mapResolution).ToString();
        scpt_cfg.robot_rot = (rotate* Mathf.Deg2Rad).ToString();
        scpt_cfg.LaunchUpdate();
        // 添加按钮点击事件处理函数
        GameObject myGameObject = gameObject;
        robotButton = myGameObject.transform.Find("Button").GetComponent<Button>();
        circle = myGameObject.transform.Find("circle").GetComponent<RawImage>();
        selectedColor = new Color32(0x52, 0xE7, 0xFF, 255);
        unselectColor = new Color32(0xD8, 0xD8, 0xD8, 255);
        robotButton.onClick.AddListener(RobotSelect);
        waypointVectorList = new List<WaypointVector>();
        waypointID = 0;

    }
    public void SetRotation(float floatValue){
        rotate = floatValue;
        scpt_cfg.robot_rot = (rotate* Mathf.Deg2Rad).ToString();
        scpt_cfg.LaunchUpdate();
        Vector3 newRotation = new Vector3(60, 0, floatValue);
        this.gameObject.transform.Find("rotate").GetComponent<RectTransform>().eulerAngles = newRotation;
    }
    public void SetPosition(Vector2 pos){
        this.position = pos;
        scpt_cfg.robot_x = (position.x*scpt_MC.mapResolution).ToString();
        scpt_cfg.robot_y = (position.y*scpt_MC.mapResolution).ToString();
        scpt_cfg.LaunchUpdate();
        for(int i = waypointVectorList.Count - 1; i >= 0; i--){
            Destroy(waypointVectorList[i].arrowObj);
            Destroy(waypointVectorList[i].obj);
            waypointVectorList.RemoveAt(i);
        }
    }
    // 销毁函数
    public void RobotSelect()
    {
        
        //findrobot
        if(scpt_EM.currentEditMode == editMode.Select){ 
            foreach(var ped in scpt_MC.PedestrainIDList){
                ped.Value.GetComponent<PedestrianAgent>().SetToDefualt();
            }
            SetToDefualt();
            if(selected == false && scpt_MC.hasAgentSelected == false){
                circle.color = selectedColor;
                selected = true;
                scpt_MC.hasAgentSelected = true;
                scpt_MC.currentAgentID = id;
            }
        }
          
    

      
    }
    public void SetToDefualt(){
        circle.color = unselectColor;
        selected = false;
        scpt_MC.hasAgentSelected = false;
        scpt_MC.currentAgentID = -1;
    }
    public void AddToWaypointList(GameObject waypointObj, GameObject arrowObj){
        if(waypointVectorList==null){waypointVectorList =new List<WaypointVector>();}
        waypointVectorList.Add(new WaypointVector(waypointID,waypointObj,arrowObj));
        waypointObj.GetComponent<WaypointAgent>().SetAgent(this.gameObject,waypointID, this.id);
        waypointID+=1;
    }

    public Vector2 GetLastWaypointPos(){
        if (waypointVectorList == null || waypointVectorList.Count==0){
            return position;
        }
        else{
            return  waypointVectorList[waypointVectorList.Count - 1].obj.GetComponent<WaypointAgent>().position;
        }
    }
}