using UnityEngine;
using UnityEngine.UI;

public class PedestrianAgent : MonoBehaviour
{
    private GameObject controller;
    private EditModeSelector scpt_EM;
    private MapControl scpt_MC;
    private Button pedestrianButton;
    private int pedestrianCounter;
    
    // 构造函数，接受pedestrianCounter作为参数
    public void Initialize(int counter)
    {
        pedestrianCounter = counter;

        scpt_EM = GameObject.Find("Controller").GetComponent<EditModeSelector>();
        scpt_MC = GameObject.Find("Controller").GetComponent<MapControl>();
        // 添加按钮点击事件处理函数
        pedestrianButton = GetComponent<Button>();
        pedestrianButton.onClick.AddListener(DestroyPedestrian);
    }

    // 销毁函数
    public void DestroyPedestrian()
    {
        if(brushMode.Pedestrian ==scpt_EM.currentBrushMode && 
            editMode.Eraser ==scpt_EM.currentEditMode){ 
            Destroy(gameObject); // 或者使用其他销毁方法，根据需求
            scpt_MC.PedestrianVectorList.DeletePedestrianVector(pedestrianCounter); 
        }
        
    }

    // 其他PedestrianAgent脚本的逻辑
}