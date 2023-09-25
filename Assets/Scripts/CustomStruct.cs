using UnityEngine;

public class PedestrianVector
{
    public int id;
    public Vector2 position;
    public GameObject obj;
    public PedestrianVector next;

    public PedestrianVector(int id, Vector2 pos, GameObject obj)
    {
        this.id = id;
        position = pos;
        this.obj = obj;
        next = null;
    }
}

public class PedestrianVectorList
{
    private PedestrianVector head;

    public void AddPedestrianVector(int id, Vector2 pos, GameObject obj)
    {
        PedestrianVector newVector = new PedestrianVector(id, pos, obj);
        
        if (head == null)
        {
            head = newVector;
        }
        else
        {
            PedestrianVector current = head;
            while (current.next != null)
            {
                current = current.next;
            }
            current.next = newVector;
            UnityEngine.Debug.Log(newVector.position);
        }
    }

    public void DeletePedestrianVector(int idToDelete)
    {
        if (head == null)
        {
            return;
        }

        if (head.id == idToDelete)
        {
            head = head.next;
            return;
        }

        PedestrianVector current = head;
        while (current.next != null)
        {
            if (current.next.id == idToDelete)
            {
                current.next = current.next.next;
                return;
            }
            current = current.next;
        }
    }
}
// 定义枚举类型
public enum brushMode
{
    None,
    Robot,
    Pedestrian,
    Waypoint,
    Wall
}
public enum editMode
{
    Pen,
    Eraser,
    Select
}
// PedestrianVectorList PedestrianVectorList = new PedestrianVectorList();

// // 添加PedestrianVector
// PedestrianVectorList.AddPedestrianVector(1, Vector2.zero, gameObject1);
// PedestrianVectorList.AddPedestrianVector(2, new Vector2(1.0f, 2.0f, 3.0f), gameObject2);

// // 删除PedestrianVector
// PedestrianVectorList.DeletePedestrianVector(1); // 删除迭代器为1的PedestrianVector