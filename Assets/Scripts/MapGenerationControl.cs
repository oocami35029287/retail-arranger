using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO; 
using System;
using System.Text;
using System.Xml;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

public struct MapData{
    public int aisleSize;
    public int storeWidth;
    public int storeHeight;
    public int doorSize;
    public float shapeDeviation;
}
public class MapGenerationControl : MonoBehaviour{
    [System.Serializable]
    public class MapYamlData
    {
        public float resolution;
        public string image;
        public float[] origin;
        public int negate;
        public float occupied_thresh;
        public float free_thresh;
    }
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
            public List<int> point1; 
            public List<int> point2;
        }
    }

    /// /////////////////////////////
    private ConfigLoader scpt_cfg;
    private MapControl scpt_MC;
    public Scrollbar aisleSizeScrollbar;
    public TMP_InputField aisleSizeInputField;

    public Scrollbar storeWidthScrollbar;
    public TMP_InputField storeWidthInputField;

    public Scrollbar storeHeightScrollbar;
    public TMP_InputField storeHeightInputField;

    public Scrollbar shapeDeviationScrollbar;
    public TMP_InputField shapeDeviationInputField;

    public Scrollbar doorSizeScrollbar;
    public TMP_InputField doorSizeInputField;

    public Button generateButton;
    public Button confirmButton;
    public Button discardButton;
    public GameObject mapPanel;
    public GameObject generatedMapWindow;
    private Process clientProcess;
    private MapData mapData;
    private ReceivedData receivedDataObject;
    private static CancellationTokenSource cancellationTokenSource;
    private bool serverThreadFlg = false;
    private bool isLoading = false;
    private int image_width;
    private int image_height;
    // Start is called before the first frame update
    void Start(){
        // Set initial values
        UpdateAisleSize(aisleSizeScrollbar.value);
        UpdateStoreWidth(storeWidthScrollbar.value);
        UpdateStoreHeight(storeHeightScrollbar.value);
        UpdateShapeDeviation(shapeDeviationScrollbar.value);
        UpdateDoorSize(doorSizeScrollbar.value);

        // Add listener methods to handle scrollbar changes
        aisleSizeScrollbar.onValueChanged.AddListener(UpdateAisleSize);
        storeWidthScrollbar.onValueChanged.AddListener(UpdateStoreWidth);
        storeHeightScrollbar.onValueChanged.AddListener(UpdateStoreHeight);
        shapeDeviationScrollbar.onValueChanged.AddListener(UpdateShapeDeviation);
        doorSizeScrollbar.onValueChanged.AddListener(UpdateDoorSize);
        generateButton.onClick.AddListener(GenerateButtonClicked);
        confirmButton.onClick.AddListener(ConfirmButtonClicked);
        discardButton.onClick.AddListener(DiscardButtonClicked);
        mapData = new MapData();
        mapData.aisleSize = 80;
        mapData.storeWidth = 800;
        mapData.storeHeight = 800;
        mapData.shapeDeviation = 0.0F;
        mapData.doorSize = 300;

        scpt_cfg = this.gameObject.GetComponent<ConfigLoader>();
        scpt_MC = this.gameObject.GetComponent<MapControl>();
        // scpt_cfg.worldDir;
    }

    // Update is called once per frame
    void Update()
    {
        // 当服务器线程完成时，生成的地图窗口需要显示出来
        if (serverThreadFlg && !isLoading)
        {
            StartCoroutine(LoadAndDisplayImageCoroutine());
            mapPanel.SetActive(false);
            generatedMapWindow.SetActive(true);
            serverThreadFlg = false;
            isLoading = true;
        }
    }

    void UpdateAisleSize(float value)
    {
        int intValue = Mathf.RoundToInt(value * (200 - 80) + 80);
        aisleSizeInputField.text = intValue.ToString();
        mapData.aisleSize = intValue;
    }

    void UpdateStoreWidth(float value)
    {
        int intValue = Mathf.RoundToInt(value * (2000 - 800) + 800);
        storeWidthInputField.text = intValue.ToString();
        mapData.storeWidth = intValue;
    }

    void UpdateStoreHeight(float value)
    {
        int intValue = Mathf.RoundToInt(value * (2000 - 800) + 800);
        storeHeightInputField.text = intValue.ToString();
        mapData.storeHeight = intValue;
    }
    void UpdateShapeDeviation(float value)
    {
        float floatValue = Mathf.RoundToInt(value * 6) * ((0.3f - 0)/6); //scale from 0 to 0.3
        shapeDeviationInputField.text = floatValue.ToString("0.00");
        mapData.shapeDeviation = floatValue;
    }
    void UpdateDoorSize(float value)
    {
        int intValue = Mathf.RoundToInt(value * (799 - 200) + 200);
        doorSizeInputField.text = intValue.ToString();
        mapData.doorSize = intValue;
    }
    void ParseReceivedData(string receivedData)
    {
        if(receivedData =="cannot found resolution more then 3 times."){
            UnityEngine.Debug.Log("error: cannot found resolution more then 3 times.");
            // scpt_MC.ShowMsgLog("error: cannot found resolution more then 3 times.");
        }
        else{
            receivedDataObject = JsonUtility.FromJson<ReceivedData>(receivedData);
            // foreach (ReceivedData.ShelfData shelf in receivedDataObject.shelf){
            //     UnityEngine.Debug.Log($"Shelf: x = {shelf.x}, y = {shelf.y}, rot = {shelf.rot}");
            // }
            // foreach (ReceivedData.WallData wall in receivedDataObject.wall){
            //     UnityEngine.Debug.Log($"Wall: [{wall.point1[0]}, {wall.point1[1]}], [{wall.point2[0]} {wall.point2[1]}]");
            // }
        }

    }
    void wallGenerate(ref XmlDocument xmlDoc,float[] pos,float rot,float length,int wall_num){
        // 在world元素下新增model元素
        XmlNode worldNode = xmlDoc.SelectSingleNode("/sdf/world"); // 修改成你的實際路徑
        if (worldNode != null)
        {
            // Create a new XmlElement for the <model> element
            XmlElement modelElement = xmlDoc.CreateElement("model");
            modelElement.SetAttribute("name", "grey_wall"+wall_num.ToString());

            // Create a new XmlElement for the <static> element and set its value
            XmlElement staticElement = xmlDoc.CreateElement("static");
            staticElement.InnerText = "1";

            // Append the <static> element to the <model> element
            modelElement.AppendChild(staticElement);

            // Create a new XmlElement for the <link> element
            XmlElement linkElement = xmlDoc.CreateElement("link");
            linkElement.SetAttribute("name", "link");

            // Create a new XmlElement for the <pose> element and set its value
            XmlElement poseElement = xmlDoc.CreateElement("pose");
            poseElement.SetAttribute("frame", "");
            poseElement.InnerText = (pos[0]/100).ToString()+" "+(pos[1]/100).ToString()+" 1.4 0 0 "+rot;

            // Append the <pose> element to the <link> element
            linkElement.AppendChild(poseElement);

            // Create a new XmlElement for the <collision> element
            XmlElement collisionElement = xmlDoc.CreateElement("collision");
            collisionElement.SetAttribute("name", "collision");

            // Create a new XmlElement for the <geometry> element
            XmlElement geometryElement = xmlDoc.CreateElement("geometry");

            // Create a new XmlElement for the <box> element
            XmlElement boxElement = xmlDoc.CreateElement("box");

            // Create a new XmlElement for the <size> element and set its value
            XmlElement sizeElement = xmlDoc.CreateElement("size");
            sizeElement.InnerText = (length/100)+" 0.2 2.8";

            // Append the <size> element to the <box> element
            boxElement.AppendChild(sizeElement);

            // Append the <box> element to the <geometry> element
            geometryElement.AppendChild(boxElement);
            
            XmlElement max_contactsElement = xmlDoc.CreateElement("max_contacts");
            max_contactsElement.InnerText = "5";
            // Append the <geometry> element to the <collision> element
            collisionElement.AppendChild(geometryElement);
            collisionElement.AppendChild(max_contactsElement);

            // Create a new XmlElement for the <visual> element
            XmlElement visualElement = xmlDoc.CreateElement("visual");
            visualElement.SetAttribute("name", "visual");

            // Create a new XmlElement for the <cast_shadows> element and set its value
            XmlElement castShadowsElement = xmlDoc.CreateElement("cast_shadows");
            castShadowsElement.InnerText = "0";

            // Append the <cast_shadows> element to the <visual> element
            visualElement.AppendChild(castShadowsElement);

            // Create a new XmlElement for the <geometry> element (for visual)
            XmlElement visualGeometryElement = xmlDoc.CreateElement("geometry");

            // Create a new XmlElement for the <box> element (for visual)
            XmlElement visualBoxElement = xmlDoc.CreateElement("box");

            // Create a new XmlElement for the <size> element and set its value (for visual)
            XmlElement visualSizeElement = xmlDoc.CreateElement("size");
            visualSizeElement.InnerText = (length/100)+" 0.2 2.8";

            // Append the <size> element to the <box> element (for visual)
            visualBoxElement.AppendChild(visualSizeElement);

            // Append the <box> element to the <geometry> element (for visual)
            visualGeometryElement.AppendChild(visualBoxElement);

            // Append the <geometry> element to the <visual> element
            visualElement.AppendChild(visualGeometryElement);



            // Create a new XmlElement for the <material> element
            XmlElement materialElement = xmlDoc.CreateElement("material");

            // Create a new XmlElement for the <script> element
            XmlElement scriptElement = xmlDoc.CreateElement("script");

            // Create a new XmlElement for the <uri> element and set its value
            XmlElement uriElement1 = xmlDoc.CreateElement("uri");
            uriElement1.InnerText = "model://grey_wall/materials/scripts";

            // Append the <uri> element to the <script> element
            scriptElement.AppendChild(uriElement1);

            // Create a new XmlElement for the <uri> element and set its value
            XmlElement uriElement2 = xmlDoc.CreateElement("uri");
            uriElement2.InnerText = "model://grey_wall/materials/textures";

            // Append the <uri> element to the <script> element
            scriptElement.AppendChild(uriElement2);
            
            // Create a new XmlElement for the <uri> element and set its value
            XmlElement uriElement3 = xmlDoc.CreateElement("name");
            uriElement3.InnerText = "vrc/grey_wall";

            // Append the <uri> element to the <script> element
            scriptElement.AppendChild(uriElement3);


            // Create a new XmlElement for the <name> element and set its value
            XmlElement nameElement = xmlDoc.CreateElement("name");
            nameElement.InnerText = "vrc/grey_wall";

            // Append the <name> element to the <material> element
            materialElement.AppendChild(nameElement);

            // Append the <script> element to the <material> element
            materialElement.AppendChild(scriptElement);

            // Append the <material> element to the <visual> element
            visualElement.AppendChild(materialElement);

            // Append the <visual> element to the <link> element
            linkElement.AppendChild(visualElement);

            // Append the <collision> element to the <link> element
            linkElement.AppendChild(collisionElement);

            // Append the <link> element to the <model> element
            modelElement.AppendChild(linkElement);

            // Append the <model> element to the <world> node
            worldNode.AppendChild(modelElement);


            UnityEngine.Debug.Log("XML file created successfully!");
        }
        else
        {
            UnityEngine.Debug.LogError("指定的父元素 'world' 未找到");
        }
    }
    void shelfGenerate(ref XmlDocument xmlDoc,float[] pos, float rot, int shelf_num){


        // 在world元素下新增model元素
        XmlNode worldNode = xmlDoc.SelectSingleNode("/sdf/world"); // 修改成你的實際路徑
        if (worldNode != null)
        {
            // 創建model元素及其子元素
            XmlElement modelElement = xmlDoc.CreateElement("model");
            modelElement.SetAttribute("name", "store_shelf_moduled_"+shelf_num.ToString());

            // link 元素及其子元素
            XmlElement linkElement = xmlDoc.CreateElement("link");
            linkElement.SetAttribute("name", "link");

            // inertial 元素及其子元素
            XmlElement inertialElement = xmlDoc.CreateElement("inertial");

            XmlElement massElement = xmlDoc.CreateElement("mass");
            massElement.InnerText = "1000";

            XmlElement inertiaElement = xmlDoc.CreateElement("inertia");

            XmlElement ixxElement = xmlDoc.CreateElement("ixx");
            ixxElement.InnerText = "840083";

            XmlElement ixyElement = xmlDoc.CreateElement("ixy");
            ixyElement.InnerText = "0";

            XmlElement ixzElement = xmlDoc.CreateElement("ixz");
            ixzElement.InnerText = "0";

            XmlElement iyyElement = xmlDoc.CreateElement("iyy");
            iyyElement.InnerText = "475500";

            XmlElement iyzElement = xmlDoc.CreateElement("iyz");
            iyzElement.InnerText = "0";

            XmlElement izzElement = xmlDoc.CreateElement("izz");
            izzElement.InnerText = "1.30208e+06";

            // 將子元素添加到父元素
            inertiaElement.AppendChild(ixxElement);
            inertiaElement.AppendChild(ixyElement);
            inertiaElement.AppendChild(ixzElement);
            inertiaElement.AppendChild(iyyElement);
            inertiaElement.AppendChild(iyzElement);
            inertiaElement.AppendChild(izzElement);

            inertialElement.AppendChild(massElement);
            inertialElement.AppendChild(inertiaElement);

            // collision 元素及其子元素
            XmlElement collisionElement = xmlDoc.CreateElement("collision");
            collisionElement.SetAttribute("name", "collision");

            XmlElement collisionGeometryElement = xmlDoc.CreateElement("geometry");

            XmlElement meshElementCollision = xmlDoc.CreateElement("mesh");

            XmlElement uriElementCollision = xmlDoc.CreateElement("uri");
            uriElementCollision.InnerText = "model://store_shelf_moduled/meshes/store_shelf_moduled.dae";

            meshElementCollision.AppendChild(uriElementCollision);
            collisionGeometryElement.AppendChild(meshElementCollision);
            collisionElement.AppendChild(collisionGeometryElement);

            // visual 元素及其子元素
            XmlElement visualElement = xmlDoc.CreateElement("visual");
            visualElement.SetAttribute("name", "visual");

            XmlElement visualGeometryElement = xmlDoc.CreateElement("geometry");

            XmlElement meshElementVisual = xmlDoc.CreateElement("mesh");

            XmlElement uriElementVisual = xmlDoc.CreateElement("uri");
            uriElementVisual.InnerText = "model://store_shelf_moduled/meshes/store_shelf_moduled.dae";

            meshElementVisual.AppendChild(uriElementVisual);
            visualGeometryElement.AppendChild(meshElementVisual);
            visualElement.AppendChild(visualGeometryElement);

            // 將 collision 和 visual 元素添加到 link 元素下
            linkElement.AppendChild(collisionElement);
            linkElement.AppendChild(visualElement);

            // self_collide 元素
            XmlElement selfCollideElement = xmlDoc.CreateElement("self_collide");
            selfCollideElement.InnerText = "0";

            // enable_wind 元素
            XmlElement enableWindElement = xmlDoc.CreateElement("enable_wind");
            enableWindElement.InnerText = "0";

            // kinematic 元素
            XmlElement kinematicElement = xmlDoc.CreateElement("kinematic");
            kinematicElement.InnerText = "0";

            // 將所有子元素添加到 link 元素下
            linkElement.AppendChild(inertialElement);
            linkElement.AppendChild(selfCollideElement);
            linkElement.AppendChild(enableWindElement);
            linkElement.AppendChild(kinematicElement);

            // static 元素
            XmlElement staticElement = xmlDoc.CreateElement("static");
            staticElement.InnerText = "1";

            // pose 元素
            XmlElement poseElement = xmlDoc.CreateElement("pose");
            poseElement.SetAttribute("frame", "");
            poseElement.InnerText = (pos[0]/100).ToString()+" "+(pos[1]/100).ToString()+" 0 0 0 "+rot*Mathf.Deg2Rad;

            // 將所有元素組裝起來
            modelElement.AppendChild(linkElement);
            modelElement.AppendChild(staticElement);
            modelElement.AppendChild(poseElement);

            // 將新建的 model 元素添加到 world 元素下
            worldNode.AppendChild(modelElement);
        }
        else
        {
            UnityEngine.Debug.LogError("指定的父元素 'world' 未找到");
        }
    }

    void StartTcpServer(CancellationToken cancellationToken)
    {
        // Set the IP address and port for the TCP server
        string ipAddressString = "127.0.0.1"; // Set the desired IP address
        int port = 12345; // Set the desired port number

        IPAddress ipAddress = IPAddress.Parse(ipAddressString);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        // Create a TcpListener
        TcpListener tcpListener = new TcpListener(localEndPoint);

        try
        {
            tcpListener.Start();
            UnityEngine.Debug.Log($"TCP Server started on {ipAddressString}:{port}");
            // scpt_MC.ShowMsgLog($"TCP Server started on {ipAddressString}:{port}");

            TcpClient client = tcpListener.AcceptTcpClient();
            UnityEngine.Debug.Log("Client connected");
            // scpt_MC.ShowMsgLog("Client connected");

            using (NetworkStream stream = client.GetStream())
            {
                // Create a StreamWriter and write path
                StreamWriter writer = new StreamWriter(stream);

                writer.Write(Application.dataPath + "/config/floor_generator/");
                writer.Flush();

                // Write map data to client
                string mapDataJson = JsonUtility.ToJson(mapData);
                writer.WriteLine(mapDataJson);
                writer.Flush();

                // Create a StringBuilder to store received data
                StringBuilder receivedDataBuilder = new StringBuilder();
                byte[] buffer = new byte[1024];

                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    receivedDataBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                }

                // Don't dispose of the NetworkStream here

                string receivedData = receivedDataBuilder.ToString();
                UnityEngine.Debug.Log($"Received data: {receivedData}");

                // Parse received data
                ParseReceivedData(receivedData);
            }
        }
        catch (SocketException e)
        {
            UnityEngine.Debug.LogError($"SocketException: {e}");
        }
        finally
        {
            // Stop the TcpListener when needed (e.g., when the application is quitting)
            tcpListener.Stop();
        }
        serverThreadFlg = true;
    }
 
    private IEnumerator LoadAndDisplayImageCoroutine()
    {
        string imagePath = "file://" + Application.dataPath + "/config/floor_generator/shelf_rectangles.jpg";

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imagePath);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);

            // 假设你有一个 RawImage 组件在场景中
            RawImage rawImageComponent = generatedMapWindow.transform.Find("RawImage").GetComponent<RawImage>();

            if (rawImageComponent != null)
            {
                rawImageComponent.texture = texture;
                rawImageComponent.rectTransform.sizeDelta = new Vector2(texture.width/texture.height*250,250);
                image_width = texture.width;
                image_height = texture.height;
                UnityEngine.Debug.Log("Image loaded and displayed successfully");
            }
            else
            {
                UnityEngine.Debug.LogError("RawImage component not found.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to load image: " + www.error);
        }

        isLoading = false;
    }
    private int getFileNum(string name,string dir){
        int i = -1;
        List<int> NumArray = new List<int>();
        // 檢查資料夾是否存在
        if (Directory.Exists(dir))
        {
            // 獲取所有 .jpg 文件
            string[] xmlFiles = Directory.GetFiles(dir, "*.jpg");
            Regex regex = new Regex($@"{name}(\d+)\.jpg");

            // 提取文件名
            foreach (string fileName in xmlFiles)
            {
                Match match = regex.Match(fileName);
                if(match.Success){
                    //UnityEngine.Debug.Log("match");
                    string numberStr = match.Groups[1].Value;
                    if (int.TryParse(numberStr, out int worldNumber))
                    {
                        NumArray.Add(worldNumber);
                        //UnityEngine.Debug.Log(sceneNumber);
                    }
                }
            }
            NumArray.Sort();
            do{
                i+=1;
            }while(i<NumArray.Count && NumArray[i]==i);
            UnityEngine.Debug.Log(i);
        }
        else
        {
            UnityEngine.Debug.LogError("資料夾不存在: " + dir);
        }
        return i;

    }
    private void CopyFile(string sourcePath, string destinationPath)
    {
        // Check if the source file exists
        if (File.Exists(sourcePath))
        {
            // Check if the destination directory exists, and create it if it doesn't
            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // Move the file to the destination directory
            File.Copy(sourcePath, destinationPath,true);

            UnityEngine.Debug.Log($"Image copied from {sourcePath} to {destinationPath}");
        }
        else
        {
            UnityEngine.Debug.LogError($"Source image does not exist at {sourcePath}");
        }
    }
    private IEnumerator CheckFileAndRefresh(string filePath)
    {
        float timeoutDuration = 2f;  // 設定超時時間（秒）
        float startTime = Time.time;

        while (!File.Exists(filePath) && Time.time - startTime < timeoutDuration)
        {
            // 等待一帧
            yield return null;
        }

        // 檢查超時
        if (Time.time - startTime >= timeoutDuration)
        {
            UnityEngine.Debug.LogWarning("File check timed out!");
        }
        else
        {
            UnityEngine.Debug.Log("File found!");

            // 在這裡添加檔案存在後的操作
            scpt_MC.RefreshMapOption();
        }
    }
    void GenerateButtonClicked()
    {

        // Convert MapData instance to JSON
        string jsonData = JsonUtility.ToJson(mapData);

        // Replace "YourExePath.exe" with the actual path to your executable file
        string exePath = Application.dataPath + "/config/floor_generator/client";
        UnityEngine.Debug.Log(Application.dataPath);
        // Create a new process to start the external executable
        ProcessStartInfo startInfo = new ProcessStartInfo(exePath);
        clientProcess = new Process();
        clientProcess.StartInfo = startInfo;
        clientProcess.Start();

        // Start a new thread to handle TCP server
        cancellationTokenSource = new CancellationTokenSource();
        Thread tcpServerThread =  new Thread(() => StartTcpServer(cancellationTokenSource.Token));
        tcpServerThread.Start();

    }

    void ConfirmButtonClicked()
    {

        // int shelf_num = 12;
        // int[] pos = new int[]{5,5};
        // float rot = 38;
        XmlDocument xmlDoc = new XmlDocument();
        string worldDir = scpt_cfg.FileDir+scpt_cfg.worldDir;
        xmlDoc.Load(Application.dataPath+"/config/empty.world");
        int shelf_num = 0;
        foreach (ReceivedData.ShelfData shelf in receivedDataObject.shelf){
            shelfGenerate(ref xmlDoc,new float[]{shelf.x,shelf.y},shelf.rot,shelf_num);
            shelf_num++;
        }
        int wall_num=0;
        foreach (ReceivedData.WallData wall in receivedDataObject.wall){
            // 計算中間點
            float[] midPoint = new float[2];
            midPoint[0] = (wall.point1[0] + wall.point2[0]) / 2.0f;
            midPoint[1] = (wall.point1[1] + wall.point2[1]) / 2.0f;
            // Debug.Log("中間點座標: (" + midPoint[0] + ", " + midPoint[1] + ")");

            // 計算轉角度
            Vector2 vector1 = new Vector2(wall.point1[0], wall.point1[1]);
            Vector2 vector2 = new Vector2(wall.point2[0], wall.point2[1]);
            
            float length = Vector2.Distance(vector1, vector2);
            // 使用Vector2.SignedAngle計算角度
            float rot = Mathf.Atan2( wall.point2[1] - wall.point1[1], wall.point2[0] - wall.point1[0]); //rad

            // Debug.Log("轉角度: " + rot + " 度");
            wallGenerate(ref xmlDoc,midPoint,rot,length,wall_num);
            wall_num++;
        
        }

        // 保存修改後的XML文件
        int world_num = getFileNum("gen_world_",$"{Application.dataPath}/config/maps/");
        xmlDoc.Save($"{worldDir}gen_world_{world_num}.world");


        //move map.pgm to turtlebot3_navigation/maps and generate yaml//////////
        MapYamlData data = new MapYamlData
        {
            //-image_height * 0.05f
            resolution = 0.05f,
            image = $"./gen_world_{world_num}.pgm",
            origin = new float[] { 0, 0, 0 },
            negate = 0,
            occupied_thresh = 0.65f,
            free_thresh = 0.196f
        };
        // Serialize the data to YAML
        var serializer = new SerializerBuilder().Build();
        string yamlContent = serializer.Serialize(data);
        File.WriteAllText($"{scpt_cfg.FileDir+scpt_cfg.mapsDir}gen_world_{world_num}.yaml", yamlContent);
        
        //copy pgm to mapsDir//////////////
        string sourcePath = Application.dataPath+"/config/floor_generator/resized_shelf_rectangles.pgm";
        string destinationPath = $"{scpt_cfg.FileDir+scpt_cfg.mapsDir}gen_world_{world_num}.pgm";
        CopyFile(sourcePath, destinationPath);
        sourcePath = Application.dataPath + "/config/floor_generator/shelf_rectangles.jpg";
        destinationPath = $"{Application.dataPath}/config/maps/gen_world_{world_num}.jpg";
        CopyFile(sourcePath, destinationPath);
        UnityEngine.Debug.Log("Image moved successfully.");
        //add the option to selector/////
        StartCoroutine(CheckFileAndRefresh(destinationPath));

        //close the gen map window//////
        mapPanel.SetActive(true);
        generatedMapWindow.SetActive(false);
        
    }

    void DiscardButtonClicked(){
        // clientProcess.Kill();
        // cancellationTokenSource.Cancel();
        mapPanel.SetActive(true);
        generatedMapWindow.SetActive(false);
    }
    void OnApplicationQuit()
    {
        // Close the server process when the application is quitting
        if (clientProcess != null && !clientProcess.HasExited)
        {
            clientProcess.Kill();
        }
    }

}
