/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WaypointAgent : MonoBehaviour
{
    private EditModeSelector scpt_EM;
    private MapControl scpt_MC;
    private Button waypointButton;
    private RawImage circle;
    private int waypointCounter;
    public Vector2 position;
    private GameObject agent;
    private int parentID;
    public int id =-1;
    // private AgentVector agentVector;
    // 构造函数，接受waypointCounter作为参数
    public void Initialize(int counter, Vector2 position,GameObject arrowIcon)
    {
        waypointCounter = counter;
        this.position = position;
        scpt_EM = GameObject.Find("Controller").GetComponent<EditModeSelector>();
        scpt_MC = GameObject.Find("Controller").GetComponent<MapControl>();
        // 添加按钮点击事件处理函数
        GameObject myGameObject = gameObject;
        waypointButton = myGameObject.transform.Find("Button").GetComponent<Button>();
        //circle = myGameObject.transform.Find("circle").GetComponent<RawImage>();
        waypointButton.onClick.AddListener(WaypointDelete);
        
        if(scpt_MC.currentAgentID!=-1){
            GameObject AgentObj;
            if(scpt_MC.currentAgentID ==-10){
                UnityEngine.Debug.Log("123");
                scpt_MC.robotIcon.GetComponent<RobotAgent>().AddToWaypointList(gameObject,arrowIcon);
            }
            else if(scpt_MC.PedestrainIDList.TryGetValue(scpt_MC.currentAgentID, out AgentObj)){
                AgentObj.GetComponent<PedestrianAgent>().AddToWaypointList(gameObject,arrowIcon);
            }
        }
        // scpt_MC.AgentVectorList.AddWaypoint(gameObject);

    }
    // 销毁函数
    public void WaypointDelete()
    {
        //destroywaypoint
        if(scpt_EM.currentEditMode == editMode.Eraser){ 
            //UnityEngine.Debug.Log($"removeAt {agent} {id}");
            List<WaypointVector> waypointVectorList;
            if(parentID==-10){
                waypointVectorList = agent.GetComponent<RobotAgent>().waypointVectorList;
            }
            else{
                waypointVectorList = agent.GetComponent<PedestrianAgent>().waypointVectorList;
            }
            int destroyNum = 0;
            bool destroyFlag = false;

            //UnityEngine.Debug.Log("123");
            for (int i = waypointVectorList.Count - 1; i >= 0; i--)
            {
                if (waypointVectorList[i].id == id)
                {
                    destroyNum = i;
                    destroyFlag = true;
                    break;
                }
            }
            if (destroyFlag)
            {
                for(int i = waypointVectorList.Count - 1; i >= destroyNum; i--){
                    Destroy(waypointVectorList[i].arrowObj);
                    Destroy(waypointVectorList[i].obj);
                    waypointVectorList.RemoveAt(i);
                }
            }
        }
        
    }
    public void SetAgent(GameObject agent,int waypointID, int parentID){
        this.agent = agent;
        this.id = waypointID;
        this.parentID = parentID;
        //UnityEngine.Debug.Log($"SetAgent {this.agent} {this.id}");
    }
}