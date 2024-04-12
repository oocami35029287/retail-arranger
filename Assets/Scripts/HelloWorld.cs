
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HelloWorld : MonoBehaviour
{
    [System.Serializable]
    public struct ReceivedData
    {
        public List<ShelfData> shelf;
        public List<WallData> wall;

        [System.Serializable]
        public struct ShelfData
        {
            public float x;
            public float y;
            public float rot;
        }

        [System.Serializable]
        public struct WallData
        {
            public List<int> points; 
            public string type;
        }
    }

    void Start()
    {
        string receivedData = "{\"shelf\":[{\"x\":117.0,\"y\":11.0,\"rot\":1.1903903446085384}],\"wall\":[{\"points\":[40, 442],\"type\":\"no_door\"}]}";

        ReceivedData receivedDataObject = JsonUtility.FromJson<ReceivedData>(receivedData);
        foreach (ReceivedData.ShelfData shelf in receivedDataObject.shelf){
            Debug.Log(shelf.x);
            Debug.Log(shelf.y);
            Debug.Log(shelf.rot);
        }
        foreach (ReceivedData.WallData wall in receivedDataObject.wall){
            Debug.Log(wall.points[0]);
            Debug.Log(wall.type);
        }
        // Now you can use receivedDataObject as needed
        Debug.Log("Hello Unity World");
    }
}