/*
 * Created by Yucheng Cheng.
 * Date: 2023/10
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class EditModeSelector : MonoBehaviour
{
    public Button eraserButton; // Eraser按钮
    public Button selectButton;

    public Button robotButton; 
    public Button pedestrianButton; 
    public Button waypointButton; 
    public Button wallButton; 
    //ped number control
    public Button incPedBtn; 
    public Button decPedBtn; 
    public int newPedNum = 1;

    private Color activeColor = Color.green; // 激活状态的颜色（蓝色）
    private Color inactiveColor = Color.white; // 非激活状态的颜色（白色）

    public editMode currentEditMode;
    //對外輸出
    private void Start()
    {
        // 添加按钮点击事件
        eraserButton.onClick.AddListener(ToggleEraser);
        selectButton.onClick.AddListener(ToggleSelect);

        robotButton.onClick.AddListener(ToggleRobot);
        pedestrianButton.onClick.AddListener(TogglePedestrian);
        waypointButton.onClick.AddListener(ToggleWaypoint);
        wallButton.onClick.AddListener(ToggleWall);

        incPedBtn.onClick.AddListener(incPed);
        decPedBtn.onClick.AddListener(decPed);

        // 初始化按钮颜色
        currentEditMode = editMode.Pedestrian;
        SetEditMode();    
    }
    //切換筆或擦子
    public void ToggleEraser()
    {
    currentEditMode = editMode.Eraser;
        SetEditMode();
    }
    public void ToggleSelect()
    {
    currentEditMode = editMode.Select;
        SetEditMode();
    }
    public void ToggleRobot(){
    currentEditMode = editMode.Robot;
        SetEditMode();
    }
    public void TogglePedestrian(){
        currentEditMode = editMode.Pedestrian;
        SetEditMode();
    }
    public void ToggleWaypoint(){
        currentEditMode = editMode.Waypoint;
        SetEditMode();
    }
    public void ToggleWall(){
        currentEditMode = editMode.Wall;
        SetEditMode();
    }

    private void SetEditMode()
    {
        
        eraserButton.image.color = inactiveColor;
        selectButton.image.color = inactiveColor;
        robotButton.image.color = inactiveColor;
        pedestrianButton.image.color = inactiveColor;
        waypointButton.image.color = inactiveColor;
        wallButton.image.color = inactiveColor;
        switch(currentEditMode){
            case(editMode.Eraser):
                eraserButton.image.color = activeColor;
                break;
            case(editMode.Select):
                selectButton.image.color = activeColor;
                break;
            case(editMode.Robot):
                robotButton.image.color = activeColor;
                break;
            case(editMode.Pedestrian):
                pedestrianButton.image.color = activeColor;
                break;
            case(editMode.Waypoint):
                waypointButton.image.color = activeColor;
                break;
            case(editMode.Wall):
                wallButton.image.color = activeColor;
                break;
            default:
                break;
        }
    }

    private void incPed(){
        newPedNum+=1;
        if(newPedNum>4){newPedNum = 4;}
        pedestrianButton.transform.Find("human_number").GetComponent<TextMeshProUGUI>().text = $"x{newPedNum}";
    }
    private void decPed(){
        newPedNum-=1;
        if(newPedNum<1){newPedNum = 1;}
        pedestrianButton.transform.Find("human_number").GetComponent<TextMeshProUGUI>().text = $"x{newPedNum}";
    }

}
