using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;


public class MapControl : MonoBehaviour
{
    public GameObject controller;
    public TextMeshProUGUI  MsgLog;
    private EditModeSelector scpt_EM;
    public TMP_Dropdown mapDropdown; // 引用下拉菜单组件
    public RawImage mapImage; // 引用用于显示地图图片的Raw Image组件
    public GameObject Mask;
    private RectTransform mask; // Mask对象的RectTransform
    private string FileDirectory = "/home/lab605/socially-store-robot";
    private string mapsDirectory = "/mars_ws/src/navigation/turtlebot3_navigation/maps/"; // 地图图片存储的目录

    private float zoomSpeed = 3.0f; // 缩放速度
    private List<string> mapItems;
    public Dictionary<int, GameObject> PedestrainIDList;
    private List<GameObject> wallLineList;
    public GameObject robotIconPrefab;
    public GameObject humanIconPrefab;
    public GameObject waypointPrefab;
    public GameObject arrowPrefab;
    public GameObject wallPrefab;
    public GameObject robotIcon;
    private int AgentCounter=0;
    public int currentAgentID = -1;//currentAgentID -1: 沒有 | -10:機器人 | >=0: 行人
    private int WaypointCounter=0;
    // public AgentVectorList AgentVectorList;
    private Vector3 mousePosition;
    public bool hasAgentSelected = false; 
    //MSG計時器
    private float timer = 0f;
    private float displayDuration = 1f;
    private bool showMessage = false;
    private bool isDrawingWall = false;
    private GameObject currentWallLine;
    void Start(){
        
        scpt_EM = controller.GetComponent<EditModeSelector>();
        //dropdown dropdown = transform.GetComponent<Dropdown>();
        mapDropdown.ClearOptions();;
        mapItems = new List<string>();
        mapItems.Add("crossing_corrider.jpg");
        mapItems.Add("map.jpg");
        List<TMP_Dropdown.OptionData> mapDropdownOptions = new List<TMP_Dropdown.OptionData>();
        foreach(string item in mapItems){
            mapDropdownOptions.Add(new TMP_Dropdown.OptionData(item));
        }
        mapDropdown.AddOptions(mapDropdownOptions);
        // 添加下拉菜单选项的监听器，以在选项更改时加载相应的地图
        mapDropdown.onValueChanged.AddListener(MapSelected);
        mapDropdown.onValueChanged.Invoke(0);

        mask = Mask.GetComponent<RectTransform>();
        PedestrainIDList = new Dictionary<int, GameObject>();
        wallLineList = new List<GameObject>();
        //添加行人vector
        // AgentVectorList = new AgentVectorList();
        robotIcon = Instantiate(robotIconPrefab, mapImage.transform);
        robotIcon.transform.SetParent(mapImage.transform);
        robotIcon.GetComponent<RobotAgent>().Initialize();
    }

    private void Update(){
        ZoomImage();
        IconManager();
        ShowMsgUpdate();

    }
    private void ShowMsgUpdate(){
        if (showMessage)
        {
            timer += Time.deltaTime;
            if (timer >= displayDuration)
            {
                MsgLog.text = ""; // 在计时器达到指定时间后将文本重新设置为空
                showMessage = false;
            }
        }

    }    
    public void ShowMsgLog(string message){
        MsgLog.text = message;
        showMessage = true;
        timer = 0f; // 重置计时器
    }
    private void MapSelected(int value){
        // 根据下拉菜单选项的值加载对应的地图图片
        string imagePath = FileDirectory + mapsDirectory + mapItems[value];

        // 加载图片并显示在Raw Image组件中
        StartCoroutine(LoadMap(imagePath));
    }
    private IEnumerator LoadMap(string imagePath){
        UnityEngine.Debug.Log(imagePath);
        // 使用Unity的WWW类加载图片
        WWW www = new WWW( "file://" + imagePath);
        yield return www;

        // 将加载的图片设置为Raw Image的纹理
        mapImage.texture = www.texture;

    }
    private void ZoomImage(){
        if(RectTransformUtility.RectangleContainsScreenPoint
        (mask, Input.mousePosition)){
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
    }
    private void IconManager(){
        switch(scpt_EM.currentEditMode){
            case(editMode.Robot):
                MoveRobot();
                break;
            case(editMode.Pedestrian):
                AddPedestrian();
                break;
            case(editMode.Waypoint):
                AddWaypoint();
                break;
            case(editMode.Wall):
                AddWall();
                break;
            default:
                break;
        }
            
    }
    private void AddPedestrian(){

        if(RectTransformUtility.RectangleContainsScreenPoint
        (mask, Input.mousePosition)){
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
                    pedestrianAgent.Initialize( AgentCounter, localPoint, scpt_EM.newPedNum);
                    PedestrainIDList.Add(AgentCounter,humanIcon);
                    //將行人放入佇列
                    // AgentVectorList.AddAgentVector(localPoint, humanIcon);
                    AgentCounter+=1;
                }
            }
        }
    }
    private void MoveRobot(){

        if(RectTransformUtility.RectangleContainsScreenPoint
        (mask, Input.mousePosition)){
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

                    ////////////要做的事
                    robotIcon.GetComponent<RobotAgent>().SetPosition(localPoint);
                    robotIcon.transform.SetParent(mapImage.transform);
                    robotIcon.transform.localPosition = localPoint;
                    //////////////////////////
                }
            }
        }
    }
    private void AddWaypoint(){
        if(RectTransformUtility.RectangleContainsScreenPoint
        (mask, Input.mousePosition)){
            if (Input.GetMouseButtonDown(0))
            {
                // 鼠标左键按下时
                mousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                // 鼠标左键释放时
                if(Input.mousePosition==mousePosition){
                    if(hasAgentSelected == true){
                        Vector2 prevPos = new Vector2(0,0);
                        bool checkflag = false;
                        if(currentAgentID == -10){
                            prevPos = robotIcon.GetComponent<RobotAgent>().GetLastWaypointPos();
                            checkflag = true;
                        }
                        else if(currentAgentID>-1){
                            GameObject AgentObj;
                            bool AgentExists = PedestrainIDList.TryGetValue(currentAgentID, out AgentObj);
                            if(AgentExists){
                                prevPos = AgentObj.GetComponent<PedestrianAgent>().GetLastWaypointPos();
                                checkflag = true;
                            }
                        }   
                        if(checkflag){
                            // 获取鼠标点击位置
                            //Vector2 mousePosition = Input.mousePosition;
                            // 将屏幕坐标转换为地图坐标
                            RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mousePosition, null, out Vector2 localPoint);

                            // 创建 waypoint 图标并设置位置
                            GameObject waypointIcon = Instantiate(waypointPrefab, mapImage.transform);
                            waypointIcon.transform.SetParent(mapImage.transform);
                            waypointIcon.transform.localPosition = localPoint;      
                            WaypointAgent waypointAgent = waypointIcon.AddComponent<WaypointAgent>();             
                            // 創建 arrow 圖標
                            
                            GameObject arrowIcon = Instantiate(arrowPrefab, mapImage.transform);
                            arrowIcon.transform.SetParent(mapImage.transform);
                            Vector2 arrowPos = prevPos + (localPoint - prevPos)/2;
                            arrowIcon.transform.localPosition = arrowPos;
                            arrowIcon.GetComponent<ArrowController>().ChangeDirection(prevPos,localPoint);    
                            //生成路徑管理器
                            waypointAgent.Initialize(WaypointCounter,localPoint,arrowIcon);      

                        }
                        else{UnityEngine.Debug.LogError("cannot find prev pos");}
                        // 将PedestrianAgent脚本附加到humanIcon并初始化它
                        //WaypointCounter+=1;
                        //AgentVectorList.AddWaypointVector(localPoint, waypointIcon);
                    }
                    else{
                        ShowMsgLog("Please select a pedestrian or robot agent.");
                    }
                }
            } 
        }
       
    }
    private void AddWall(){
        if (RectTransformUtility.RectangleContainsScreenPoint(mask, Input.mousePosition))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 鼠标左键按下时
                mousePosition = Input.mousePosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, mousePosition, null, out Vector2 startPoint);

                // 创建新的线条对象
                currentWallLine = Instantiate(wallPrefab, mapImage.transform);
                currentWallLine.transform.SetParent(mapImage.transform);
                currentWallLine.GetComponent<WallController>().SetStartPoint(startPoint);
                currentWallLine.GetComponent<WallController>().SetEndPoint(startPoint+new Vector2(0.1f,0.1f));

                isDrawingWall = true; // 标记开始绘制   
                Mask.GetComponent<ScrollRect>().horizontal = false;
                Mask.GetComponent<ScrollRect>().vertical = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                // 鼠标左键释放时完成线条
                isDrawingWall = false;
                Mask.GetComponent<ScrollRect>().horizontal = true;
                Mask.GetComponent<ScrollRect>().vertical = true;
                wallLineList.Add(currentWallLine);
            }

            // 如果正在绘制，更新线条的终点
            if (isDrawingWall)
            {
                Vector2 currentMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(mapImage.rectTransform, Input.mousePosition, null, out currentMousePos);
                currentWallLine.GetComponent<WallController>().SetEndPoint(currentMousePos);
            }
        }
    }
}
