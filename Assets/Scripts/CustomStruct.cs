using UnityEngine;
using System.Collections.Generic;

public class WaypointVector
{
    public int id;
    public GameObject obj;
    public GameObject arrowObj;
    public WaypointVector(int id, GameObject obj, GameObject arrowObj)
    {
        this.id = id;
        this.obj = obj;
        this.arrowObj = arrowObj;
    }
}

// 定义枚举类型
public enum editMode
{
    None,
    Robot,
    Pedestrian,
    Waypoint,
    Wall,

    Eraser,
    Select
}

// AgentVectorList AgentVectorList = new AgentVectorList();

// // 添加AgentVector
// AgentVectorList.AddAgentVector(1, Vector2.zero, gameObject1);
// AgentVectorList.AddAgentVector(2, new Vector2(1.0f, 2.0f, 3.0f), gameObject2);

// // 删除AgentVector
// AgentVectorList.DeleteAgentVector(1); // 删除迭代器为1的AgentVector