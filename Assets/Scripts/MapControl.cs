using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;


public class MapControl : MonoBehaviour
{
    public GameObject controller;
    private EditModeSelector scpt_EM;
    public RectTransform mask; // Mask对象的RectTransform
    public TMP_Dropdown dropdown; // 引用下拉菜单组件
    public RawImage mapImage; // 引用用于显示地图图片的Raw Image组件
    private string FileDirectory = "/home/lab605/socially-store-robot";
    private string mapsDirectory = "/mars_ws/src/navigation/turtlebot3_navigation/maps/"; // 地图图片存储的目录
    private float zoomSpeed = 3.0f; // 缩放速度
    private List<string> items;
    public GameObject humanIconPrefab;
    private int pedestrianCounter=0;
    public PedestrianVectorList PedestrianVectorList;
    Vector3 mousePosition;
    void Start()
    {
        
        scpt_EM = controller.GetComponent<EditModeSelector>();
        //dropdown dropdown = transform.GetComponent<Dropdown>();
        dropdown.ClearOptions();;
        items = new List<string>();
        items.Add("crossing_corrider.jpg");
        items.Add("map.jpg");
        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
        foreach(string item in items){
            dropdownOptions.Add(new TMP_Dropdown.OptionData(item));
        }
        dropdown.AddOptions(dropdownOptions);
        // 添加下拉菜单选项的监听器，以在选项更改时加载相应的地图
        dropdown.onValueChanged.AddListener(DropdownItemSelected);
        dropdown.onValueChanged.Invoke(0);

        //添加行人vector
        PedestrianVectorList = new PedestrianVectorList();
        
    }

    private void Update()
    {
        ZoomImage();
        IconManager();
    }
    private void ZoomImage(){
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        // 如果滚轮滚动不为零，则进行缩放操作
        if (scrollDelta != 0)
        {
            // 获取当前 Raw Image 的尺寸
            Vector2 currentSize = mapImage.transform.localScale;

            // 计算缩放后的尺寸
            float newSizeX = currentSize.x * (1 + scrollDelta * zoomSpeed);
            float newSizeY = currentSize.y * (1 + scrollDelta * zoomSpeed);

            // 限制最小尺寸，避免缩放过小
            newSizeX = Mathf.Max(newSizeX, 1f);
            newSizeY = Mathf.Max(newSizeY, 1f);

            // 设置 Raw Image 的尺寸
            mapImage.transform.localScale = new Vector3(newSizeX, newSizeY,0f);
        }

    }
    private void IconManager(){
        switch(scpt_EM.currentBrushMode){
            case(brushMode.None):
                break;
            case(brushMode.Robot):
                break;
            case(brushMode.Pedestrian):
                if(scpt_EM.currentEditMode ==editMode.Pen)
                    AddPedestrian();
                else if(scpt_EM.currentEditMode ==editMode.Eraser)
                    break;
                    //交由各個pedestrian agent執行delete pedestrian的任務
                break;
            case(brushMode.Waypoint):
                break;
            case(brushMode.Wall):
                break;
            default:
                break;
        }
            
    }
    private void AddPedestrian(){
        if (Input.GetMouseButtonDown(0))
        {
            // 鼠标左键按下时
            mousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // 鼠标左键释放时
            if(Input.mousePosition==mousePosition){
                // 获取鼠标点击位置
                //Vector2 mousePosition = Input.mousePosition;
                // 将屏幕坐标转换为地图坐标
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mousePosition, null, out Vector2 localPoint);

                // 创建 Human 图标并设置位置
                
                GameObject humanIcon = Instantiate(humanIconPrefab, mapImage.transform);
                humanIcon.transform.SetParent(mapImage.transform);
                humanIcon.transform.localPosition = localPoint;

                // 将PedestrianAgent脚本附加到humanIcon并初始化它
                PedestrianAgent pedestrianAgent = humanIcon.AddComponent<PedestrianAgent>();
                pedestrianAgent.Initialize(pedestrianCounter);
                //將行人放入佇列
                PedestrianVectorList.AddPedestrianVector(pedestrianCounter, localPoint, humanIcon);
                pedestrianCounter+=1;
            }
        }
    }

    private void DropdownItemSelected(int value)
    {
        // 根据下拉菜单选项的值加载对应的地图图片
        string imagePath = FileDirectory + mapsDirectory + items[value];

        // 加载图片并显示在Raw Image组件中
        StartCoroutine(LoadMap(imagePath));
    }

    private IEnumerator LoadMap(string imagePath)
    {
        UnityEngine.Debug.Log(imagePath);
        // 使用Unity的WWW类加载图片
        WWW www = new WWW( "file://" + imagePath);
        yield return www;

        // 将加载的图片设置为Raw Image的纹理
        mapImage.texture = www.texture;

    }
}
