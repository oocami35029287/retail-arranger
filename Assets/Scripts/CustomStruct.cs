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


// LaunchItemUtility.LaunchItem[] launchItems = LaunchItemUtility.LaunchItems;

// // 访问数组的元素
// LaunchItemUtility.LaunchItem firstItem = launchItems[0];
// bool flag = firstItem.Flag;
// string tool = firstItem.Tool;
// string command = firstItem.Command;