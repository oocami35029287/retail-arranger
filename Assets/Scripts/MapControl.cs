/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

public class MapControl : MonoBehaviour
{
    public TextMeshProUGUI  MsgLog;
    private EditModeSelector scpt_EM;
    private DockerControl scpt_DC;
    private ConfigLoader scpt_cfg;

    public TMP_Dropdown mapDropdown; // 引用下拉菜单组件
    public TMP_Dropdown sceneDropdown; 
    public RawImage mapImage; // 引用用于显示地图图片的Raw Image组件
    public GameObject Mask;
    private RectTransform mask; // Mask对象的RectTransform
    [System.NonSerialized]
    public float mapResolution;
    private Vector2 mapOrigin;
    private float zoomSpeed = 3.0f; // 缩放速度
    private List<string> mapItems;
    private List<string> sceneItems;
    public Dictionary<int, GameObject> PedestrainIDList;
    public List<GameObject> wallLineList;
    public GameObject robotIconPrefab;
    public GameObject humanIconPrefab;
    public GameObject waypointPrefab;
    public GameObject arrowPrefab;
    public GameObject wallPrefab;
    public TMP_InputField inputField;
    [System.NonSerialized]
    public GameObject robotIcon;
    private int AgentCounter=0;
    [System.NonSerialized]
    public int currentAgentID = -1;//currentAgentID -1: 沒有 | -10:機器人 | >=0: 行人
    private int WaypointCounter=0;
    // public AgentVectorList AgentVectorList;
    private Vector3 mousePosition;
    [System.NonSerialized]
    public bool hasAgentSelected = false; 
    //MSG計時器
    private float timer = 0f;
    private float displayDuration = 1f;
    private bool showMessage = false;
    private bool isDrawingWall = false;
    private GameObject currentWallLine;

    public Button clearButton;
    void Start(){
        
        scpt_EM = this.gameObject.GetComponent<EditModeSelector>();
        scpt_DC = this.gameObject.GetComponent<DockerControl>();
        scpt_cfg = this.gameObject.GetComponent<ConfigLoader>();

        mask = Mask.GetComponent<RectTransform>();
        PedestrainIDList = new Dictionary<int, GameObject>();
        wallLineList = new List<GameObject>();
        //////////
        //添加行人vector
        // AgentVectorList = new AgentVectorList();
        robotIcon = Instantiate(robotIconPrefab, mapImage.transform);
        robotIcon.transform.SetParent(mapImage.transform);
        robotIcon.GetComponent<RobotAgent>().Initialize();
        //map的設置
        mapResolution = 0.05f;
        mapOrigin = Vector2.zero;

        //map 下拉選單
        mapDropdown.ClearOptions();
        mapItems = scpt_cfg.mapItems;
        List<TMP_Dropdown.OptionData> mapDropdownOptions = new List<TMP_Dropdown.OptionData>();
        foreach(string item in mapItems){
            mapDropdownOptions.Add(new TMP_Dropdown.OptionData(item));
        }
        mapDropdown.AddOptions(mapDropdownOptions);
        mapDropdown.onValueChanged.AddListener(MapSelected);
        mapDropdown.onValueChanged.Invoke(0);
        
        //scene 下拉選單
        sceneDropdown.ClearOptions();
        sceneItems = scpt_cfg.sceneItems;
        List<TMP_Dropdown.OptionData> sceneDropdownOptions = new List<TMP_Dropdown.OptionData>();
        foreach(string item in sceneItems){
            sceneDropdownOptions.Add(new TMP_Dropdown.OptionData(item));
        }
        sceneDropdown.AddOptions(sceneDropdownOptions);
        sceneDropdown.onValueChanged.AddListener(SceneSelected);
        sceneDropdown.onValueChanged.Invoke(0);
        //input field
        inputField.onSubmit.AddListener(RotateRobot);


        //button
        clearButton.onClick.AddListener(ClearScene);

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
    public void RefreshSceneOption(){
        scpt_cfg.LoadScene();
        sceneDropdown.ClearOptions();
        sceneItems = scpt_cfg.sceneItems;
        List<TMP_Dropdown.OptionData> sceneDropdownOptions = new List<TMP_Dropdown.OptionData>();
        foreach(string item in sceneItems){
            sceneDropdownOptions.Add(new TMP_Dropdown.OptionData(item));
        }
        sceneDropdown.AddOptions(sceneDropdownOptions);
    }    
    public void ShowMsgLog(string message){
        MsgLog.text = message;
        showMessage = true;
        timer = 0f; // 重置计时器
    }
    private void MapSelected(int value){
        // 根据下拉菜单选项的值加载对应的地图图片
        // 加载图片并显示在Raw Image组件中
        LoadMapYaml(mapItems[value]);
        StartCoroutine(LoadMap(mapItems[value]));
        scpt_cfg.curMapItem = mapItems[value];
        scpt_cfg.LaunchUpdate();
    }
    private void SceneSelected(int value){
        scpt_cfg.curSceneItem = sceneItems[value];
        scpt_cfg.LaunchUpdate();
        LoadScene(sceneItems[value]);
    }
    private void LoadMapYaml(string mapYaml){
        string yamlPath = scpt_cfg.FileDir + scpt_cfg.mapsDir + mapYaml+".yaml";  
        //UnityEngine.Debug.Log(yamlPath);      
        // 讀取map.yaml文件
        if (File.Exists(yamlPath)){
            string yamlText = File.ReadAllText(yamlPath);
            //Debug.Log("File Content: " + yamlText);
            
            // 在这里继续处理文件内容
            // 使用正則表達式提取resolution和origin的值
            string resolutionPattern = @"resolution:\s+(\d+\.\d+)";
            string originPattern = @"origin:\s+\[(-?\d+\.\d+),\s*(-?\d+\.\d+),\s*(-?\d+\.\d+)\]";


            Match resolutionMatch = Regex.Match(yamlText, resolutionPattern);
            if (resolutionMatch.Success)
            {
                mapResolution = float.Parse(resolutionMatch.Groups[1].Value);
            }

            Match originMatch = Regex.Match(yamlText, originPattern);
            if (originMatch.Success)
            {
                float x = float.Parse(originMatch.Groups[1].Value);
                float y = float.Parse(originMatch.Groups[2].Value);
                float z = float.Parse(originMatch.Groups[3].Value);
                mapOrigin = new Vector2(x, y);
            }

            // 在Unity的控制台中顯示解析結果
            //UnityEngine.Debug.Log($"Resolution: {mapResolution}");
            //UnityEngine.Debug.Log($"Origin: {mapOrigin}");
        }
        else{
            Debug.LogError("File not found: " + yamlPath);
        }

        
    }
    private IEnumerator LoadMap(string map){
        string imagePath = scpt_cfg.showMaps + map+".jpg";
        //UnityEngine.Debug.Log(imagePath);
        // 使用Unity的WWW类加载图片
        WWW www = new WWW( "file://" + imagePath);
        yield return www;

        // 将加载的图片设置为Raw Image的纹理
        mapImage.texture = www.texture;
        mapImage.rectTransform.sizeDelta = new Vector2(www.texture.width,www.texture.height);
    }
    private void LoadScene(string scene){
        string scenePath = scpt_cfg.FileDir + scpt_cfg.sceneDir + scene+".xml";
        if (File.Exists(scenePath)){
            string sceneText = File.ReadAllText(scenePath);
            // 加載XML文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sceneText);
            //Debug.Log(sceneText);

            // 在这里继续处理文件内容
            ClearScene();
            // 提取obstacle元素的值
            XmlNodeList obstacleNodes = xmlDoc.SelectNodes("//obstacle");
            foreach (XmlNode obstacleNode in obstacleNodes)
            {
                float x1 = float.Parse(obstacleNode.Attributes["x1"].Value)/mapResolution;
                float x2 = float.Parse(obstacleNode.Attributes["x2"].Value)/mapResolution;
                float y1 = float.Parse(obstacleNode.Attributes["y1"].Value)/mapResolution;
                float y2 = float.Parse(obstacleNode.Attributes["y2"].Value)/mapResolution;

                //Debug.Log($"Obstacle: x1 = {x1}, x2 = {x2}, y1 = {y1}, y2 = {y2}");
                GameObject wallLine = Instantiate(wallPrefab, mapImage.transform);
                wallLine.transform.SetParent(mapImage.transform);
                wallLine.GetComponent<WallController>().SetStartPoint(new Vector2(x1,y1));
                wallLine.GetComponent<WallController>().SetEndPoint(new Vector2(x2,y2));
                wallLineList.Add(wallLine);
            }

            // 提取waypoint元素的值
            Dictionary<string, Vector2> waypoints = new Dictionary<string, Vector2>();
            XmlNodeList waypointNodes = xmlDoc.SelectNodes("//waypoint");
            foreach (XmlNode waypointNode in waypointNodes)
            {
                string id = waypointNode.Attributes["id"].Value;
                float x = float.Parse(waypointNode.Attributes["x"].Value)/mapResolution;
                float y = float.Parse(waypointNode.Attributes["y"].Value)/mapResolution;
                
                waypoints[id] = new Vector2(x, y);
            }

            // 提取agent元素的值
            XmlNodeList agentNodes = xmlDoc.SelectNodes("//agent");
            foreach (XmlNode agentNode in agentNodes)
            {
                float x = float.Parse(agentNode.Attributes["x"].Value)/mapResolution;
                float y = float.Parse(agentNode.Attributes["y"].Value)/mapResolution;
                Vector2 pedPosition = new Vector2(x,y);
                int n = int.Parse(agentNode.Attributes["n"].Value);
                int type = int.Parse(agentNode.Attributes["type"].Value);
                // 更改robot 位置
                if(type==2){
                    robotIcon.transform.localPosition = pedPosition;
                    //robotIcon.GetComponent<RobotAgent>().position = pedPosition;
                    robotIcon.GetComponent<RobotAgent>().SetPosition(pedPosition);
                    string rValue = agentNode.Attributes["r"]?.Value; 
                    if(rValue != null && float.TryParse(rValue, out float rotate)){
                        robotIcon.GetComponent<RobotAgent>().SetRotation(rotate*Mathf.Rad2Deg);
                    }
                    else{robotIcon.GetComponent<RobotAgent>().SetRotation(0);}
                }
                // 创建 Human 图标并设置位置
                if(type==0 && n!=0){
                    GameObject humanIcon = Instantiate(humanIconPrefab, mapImage.transform);
                    humanIcon.transform.SetParent(mapImage.transform);
                    humanIcon.transform.localPosition = pedPosition;
                    // 将PedestrianAgent脚本附加到humanIcon并初始化它
                    PedestrianAgent pedestrianAgent = humanIcon.AddComponent<PedestrianAgent>();
                    pedestrianAgent.Initialize( AgentCounter, pedPosition, n);
                    PedestrainIDList.Add(AgentCounter,humanIcon);
                    currentAgentID = AgentCounter;
                    AgentCounter+=1;
                }

                
                XmlNodeList addWaypointNodes = agentNode.SelectNodes("addwaypoint");
                foreach (XmlNode addWaypointNode in addWaypointNodes)
                {
                    string waypointId = addWaypointNode.Attributes["id"].Value;
                    if (waypoints.ContainsKey(waypointId))
                    {
                        Vector2 waypointPos = waypoints[waypointId];
                        
                        //找到前一個waypoint的位置
                        Vector2 prevPos = new Vector2(0,0);
                        GameObject AgentObj;
                        bool AgentExists = PedestrainIDList.TryGetValue(currentAgentID, out AgentObj);
                        if(AgentExists){
                            prevPos = AgentObj.GetComponent<PedestrianAgent>().GetLastWaypointPos();
                        }
                        // 创建 waypoint 图标并设置位置
                        GameObject waypointIcon = Instantiate(waypointPrefab, mapImage.transform);
                        waypointIcon.transform.SetParent(mapImage.transform);
                        waypointIcon.transform.localPosition = waypointPos;      
                        WaypointAgent waypointAgent = waypointIcon.AddComponent<WaypointAgent>();             
                        // 創建 arrow 圖標
                        GameObject arrowIcon = Instantiate(arrowPrefab, mapImage.transform);
                        arrowIcon.transform.SetParent(mapImage.transform);
                        Vector2 arrowPos = prevPos + (waypointPos - prevPos)/2;
                        arrowIcon.transform.localPosition = arrowPos;
                        arrowIcon.GetComponent<ArrowController>().ChangeDirection(prevPos,waypointPos);    
                        //生成路徑管理器
                        waypointAgent.Initialize(WaypointCounter,waypointPos,arrowIcon);      
                        
                        WaypointCounter+=1;
    
                    }
                }
                currentAgentID = -1;
              
        }
        }
        else{
            Debug.LogError("File not found: " + scenePath);
        }

    }
    private void ClearScene(){
        for (int i = wallLineList.Count - 1; i >= 0; i--)
        {
            Destroy(wallLineList[i]);
            wallLineList.RemoveAt(i);
        }
        List<GameObject> PedestrainList = new List<GameObject>();
        foreach (var ped in PedestrainIDList)
        {
            if(ped.Value!=null){
                PedestrainList.Add(ped.Value);
            }
        }
        for (int i = PedestrainList.Count - 1; i >= 0; i--)
        {
            PedestrainList[i].GetComponent<PedestrianAgent>().DeletePedestrian();
        }
        PedestrainIDList.Clear();


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
    private void RotateRobot(string inputText){
        // 在这里处理输入文本
        float floatValue;
        if (float.TryParse(inputText, out floatValue)){
            // Conversion was successful
            while(floatValue<0){
                floatValue+=720;
            }
            floatValue = floatValue%360;
            robotIcon.GetComponent<RobotAgent>().SetRotation(floatValue);
            //inputField.textComponent.text = floatValue.ToString("F0");

            Debug.Log("输入文本：" + floatValue);
        }
        else{
            // Conversion failed, handle the error
            Debug.LogError("Failed to convert input text to float.");
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
                        WaypointCounter+=1;
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
