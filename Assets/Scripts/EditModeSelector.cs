using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;

public class EditModeSelector : MonoBehaviour
{
    public Button penButton; // Pen按钮
    public Button eraserButton; // Eraser按钮
    public Button selectButton;

    public Button robotButton; 
    public Button pedestrianButton; 
    public Button waypointButton; 
    public Button wallButton; 

    private Color activeColor = Color.green; // 激活状态的颜色（蓝色）
    private Color inactiveColor = Color.white; // 非激活状态的颜色（白色）

    public editMode currentEditMode;
    public brushMode currentBrushMode;
    //對外輸出
    private void Start()
    {
        // 添加按钮点击事件
        penButton.onClick.AddListener(TogglePen);
        eraserButton.onClick.AddListener(ToggleEraser);
        selectButton.onClick.AddListener(ToggleSelect);

        robotButton.onClick.AddListener(ToggleRobot);
        pedestrianButton.onClick.AddListener(TogglePedestrian);
        waypointButton.onClick.AddListener(ToggleWaypoint);
        wallButton.onClick.AddListener(ToggleWall);

        // 初始化按钮颜色
        currentEditMode = editMode.Pen;
        SetEditMode();        
        currentBrushMode = brushMode.None;
        SetBrushMode();
    }
    //切換筆或擦子
    public void TogglePen()
    {
        currentEditMode = editMode.Pen;
        SetEditMode();
    }

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
    private void SetEditMode()
    {
        penButton.image.color = inactiveColor;
        eraserButton.image.color = inactiveColor;
        selectButton.image.color = inactiveColor;
        switch(currentEditMode){
            case(editMode.Pen):
                penButton.image.color = activeColor;
                break;
            case(editMode.Eraser):
                eraserButton.image.color = activeColor;
                break;
            case(editMode.Select):
                selectButton.image.color = activeColor;
                break;
            default:
                break;
        }
    }

    //切換不同的畫筆模式 機器人, 路人, waypoin, wall
    public void ToggleRobot()
    {
        if(currentBrushMode == brushMode.Robot){
            currentBrushMode =  brushMode.None;   
        }
        else{
            currentBrushMode = brushMode.Robot;
        }
        SetBrushMode();
    }

    public void TogglePedestrian()
    {
        if(currentBrushMode == brushMode.Pedestrian){
            currentBrushMode =  brushMode.None;   
        }
        else{
            currentBrushMode = brushMode.Pedestrian;
        }
        SetBrushMode();
    }
        public void ToggleWaypoint()
    {
        if(currentBrushMode == brushMode.Waypoint){
            currentBrushMode =  brushMode.None;   
        }
        else{
            currentBrushMode = brushMode.Waypoint;
        }
        SetBrushMode();
    }
        public void ToggleWall()
    {
        if(currentBrushMode == brushMode.Wall){
            currentBrushMode =  brushMode.None;   
        }
        else{
            currentBrushMode = brushMode.Wall;
        }
        SetBrushMode();
    }
    private void SetBrushMode()
    {
        robotButton.image.color = inactiveColor;
        pedestrianButton.image.color = inactiveColor;
        waypointButton.image.color = inactiveColor;
        wallButton.image.color = inactiveColor;
        switch(currentBrushMode){
            case(brushMode.None):
                break;
            case(brushMode.Robot):
                robotButton.image.color = activeColor;
                break;
            case(brushMode.Pedestrian):
                pedestrianButton.image.color = activeColor;
                break;
            case(brushMode.Waypoint):
                waypointButton.image.color = activeColor;
                break;
            case(brushMode.Wall):
                wallButton.image.color = activeColor;
                break;
            default:
                break;
        }
    }

}
