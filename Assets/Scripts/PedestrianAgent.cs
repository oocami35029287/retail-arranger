/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PedestrianAgent : MonoBehaviour
{
    public int id;
    public Vector2 position;
    public int agentNumber;
    private int waypointID;
    private bool selected = false;

    private GameObject controller;
    private EditModeSelector scpt_EM;
    private MapControl scpt_MC;
    private Button pedestrianButton;
    private RawImage circle;
    private Color32 selectedColor;
    private Color32 unselectColor;
    
    // public AgentVector agentVector;
    public List<WaypointVector> waypointVectorList;
    //建構函數
    public void Initialize(int id, Vector2 position, int agentNumber)
    {
        this.position = position;
        this.id = id;
        this.agentNumber = agentNumber;
        scpt_EM = GameObject.Find("Controller").GetComponent<EditModeSelector>();
        scpt_MC = GameObject.Find("Controller").GetComponent<MapControl>();
        // 添加按钮点击事件处理函数
        GameObject myGameObject = gameObject;
        pedestrianButton = myGameObject.transform.Find("Button").GetComponent<Button>();
        circle = myGameObject.transform.Find("circle").GetComponent<RawImage>();
        selectedColor = new Color32(0x52, 0xE7, 0xFF, 255);
        unselectColor = new Color32(0xD8, 0xD8, 0xD8, 255);
        pedestrianButton.onClick.AddListener(PedestrianSelect);
        waypointVectorList = new List<WaypointVector>();
        waypointID = 0;
        myGameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"x{agentNumber}";
    }

    public void PedestrianSelect()
    {
        
        //findPedestrian
        if(scpt_EM.currentEditMode == editMode.Select){ 
            foreach(var ped in scpt_MC.PedestrainIDList){
                ped.Value.GetComponent<PedestrianAgent>().SetToDefualt();
            }
            scpt_MC.robotIcon.GetComponent<RobotAgent>().SetToDefualt();
            if(selected == false && scpt_MC.hasAgentSelected == false){
                circle.color = selectedColor;
                selected = true;
                // this.agentVector = agentVec;   
                scpt_MC.hasAgentSelected = true;
                scpt_MC.currentAgentID = id;
                // scpt_MC.AgentVectorList.AgentSelected(agentVector);
            }
            // else if(selected == true) {
            //     circle.color = unselectColor;
            //     selected = false;
            //     scpt_MC.hasAgentSelected = false;
            //     scpt_MC.currentAgentID = -1;
            //         // scpt_MC.AgentVectorList.AgentUnselect(agentVector);
                
            // }
        }
          
    
        //destroyPedestrian
        if(scpt_EM.currentEditMode == editMode.Eraser){ 
            DeletePedestrian();    
        }
      
    }
    public void DeletePedestrian(){
        for(int i = waypointVectorList.Count - 1; i >= 0; i--){
            Destroy(waypointVectorList[i].arrowObj);
            Destroy(waypointVectorList[i].obj);
            waypointVectorList.RemoveAt(i);
        }
        if(selected){
            scpt_MC.hasAgentSelected = false;
        }
        Destroy(this.gameObject); // 或者使用其他销毁方法，根据需求
        if(scpt_MC.PedestrainIDList.ContainsKey(id)){
            scpt_MC.PedestrainIDList.Remove(id);
        }
        else{UnityEngine.Debug.LogError("caanot find pedestrain in IDList.");}
        // scpt_MC.AgentVectorList.DeleteAgentVector(agentVector); 
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