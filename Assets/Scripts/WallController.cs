/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using UnityEngine;
using UnityEngine.UI;

public class WallController : MonoBehaviour
{
    private EditModeSelector scpt_EM;
    private RectTransform rectTrans;
    private RectTransform lineTrans;

    public RawImage rect; // 方形图片
    public Vector2 startPoint;
    public Vector2 endPoint;
    public float lineWidth = 2.0f; // 箭头宽度
    private float lineLength; // 箭头高度

    
    //构造函数，用于设置宽度和高度
    public void ChangeDirection(Vector2 startPoint, Vector2 endPoint)
    {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        SetWall();
    }
    public void SetStartPoint(Vector2 startPoint){
        scpt_EM = GameObject.Find("Controller").GetComponent<EditModeSelector>();
        rectTrans = rect.GetComponent<RectTransform>();
        lineTrans = this.GetComponent<RectTransform>();
        this.startPoint = startPoint;
    }
    public void SetEndPoint(Vector2 endPoint){
        this.endPoint = endPoint;
        SetWall();
    }
    private void Update(){
        EraseWall();
    }
    private void EraseWall(){
        if (scpt_EM.currentEditMode == editMode.Eraser)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, Input.mousePosition, null, out localPoint);
                if( rectTrans.rect.Contains(localPoint)){
                    Destroy(this.gameObject);
                }
                
            }
        }
    }
    // 创建箭头
    private void SetWall()
    {
        //方形的佈置
        rectTrans.anchoredPosition = Vector2.zero;
        lineLength = Vector2.Distance(startPoint, endPoint);
        rectTrans.sizeDelta = new Vector2(lineLength, lineWidth);
        //整體的佈置
        lineTrans.anchoredPosition = startPoint+(endPoint-startPoint)/2;
        float angle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x) * Mathf.Rad2Deg;
        lineTrans.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void DestroyThis(){
        Destroy(gameObject);
    }

    // // 更新箭头的大小
    // private void Update()
    // {

    // }
}