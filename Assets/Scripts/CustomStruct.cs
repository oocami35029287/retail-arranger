/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;




public class WaypointVector{
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

// 定义editMode枚举类型
public enum editMode{
    None,
    Robot,
    Pedestrian,
    Waypoint,
    Wall,

    Eraser,
    Select
}


