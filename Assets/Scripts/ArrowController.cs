/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour
{
    public RawImage rect; // 方形图片
    public RawImage tri; // 三角形图片
    public Vector2 startPoint;
    public Vector2 endPoint;
    public float arrowWidth = 2.0f; // 箭头宽度
    public float spare = 28.0f;
    private float arrowLength; // 箭头高度

    
    // 构造函数，用于设置宽度和高度
    public void ChangeDirection(Vector2 startPoint, Vector2 endPoint)
    {
        this.startPoint = startPoint;
        this.endPoint = endPoint;
        SetArrow();
    }

    private void Update()
    {
        SetArrow();
    }

    // 创建箭头
    private void SetArrow()
    {
        RectTransform rectTrans = rect.GetComponent<RectTransform>();
        RectTransform triTrans = tri.GetComponent<RectTransform>();
        //方形的佈置
        rectTrans.anchoredPosition = Vector2.zero;
        arrowLength = Vector2.Distance(startPoint, endPoint)-spare;
        if(arrowLength<5.0f){arrowLength = 5.0f;}
        rectTrans.sizeDelta = new Vector2(arrowLength, arrowWidth);
        //三角形的佈置
        triTrans.anchoredPosition = new Vector2(arrowLength / 2.0f, 0.0f);
        //triangle.sizeDelta = new Vector2(arrowWidth, arrowHeight / 2.0f);
        //整體的佈置
        RectTransform arrowTrans = this.GetComponent<RectTransform>();
        arrowTrans.anchoredPosition = startPoint+(endPoint-startPoint)/2;
        float angle = Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x) * Mathf.Rad2Deg;
        arrowTrans.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void DestroyThis(){
        Destroy(gameObject);
    }
    // // 更新箭头的大小
    // private void Update()
    // {

    // }
}