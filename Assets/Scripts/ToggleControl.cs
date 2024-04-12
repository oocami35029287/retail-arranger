using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConfigLoader.LaunchOption.LaunchOpt;
using TMPro;

public class ToggleControl : MonoBehaviour
{
    public RectTransform moduleRectTransform;
    public RectTransform imgRectTransform;
    public GameObject buttonPrefab; // 需要指定按鈕的預置物
    private int numberOfButtons = 0; // 按鈕的數量
    private Button[] buttons; // 儲存實例化的按鈕
    private List<ConfigLoader.LaunchOptionCheck.OptCheck> LaunchOpts;
    private ConfigLoader scpt_cfg;

    public void noInit(){
        numberOfButtons = 0;
    }
    public void initOpt(ref List<ConfigLoader.LaunchOptionCheck.OptCheck> launchOptsIn, ConfigLoader scpt_cfg)
    {
        this.scpt_cfg = scpt_cfg;
        LaunchOpts = launchOptsIn;
        numberOfButtons = LaunchOpts.Count;
        buttons = new Button[numberOfButtons]; // 初始化按鈕數組

        // 動態生成按鈕
        for (int i = 0; i < numberOfButtons; i++)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, transform); // 在父物件（本腳本所附加的GameObject）下生成按鈕
            RectTransform buttonRectTransform = buttonGO.GetComponent<RectTransform>(); // 取得按鈕的RectTransform組件
            Button buttonComponent = buttonGO.GetComponent<Button>(); // 取得按鈕的Button組件
            buttons[i] = buttonComponent; // 將按鈕添加到按鈕數組中

            // 設置按鈕的位置
            buttonRectTransform.anchoredPosition = new Vector2(25f, -25-(i * (buttonRectTransform.rect.height+2)));
            // 添加按鈕點擊事件
            int buttonIndex = i; // 在委派中使用局部變量需要額外設置
            buttonComponent.onClick.AddListener(() => OnButtonClick(buttonIndex));
             buttonComponent.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = LaunchOpts[i].Name;
        }
        // 设置第一个按钮为已选中状态
        OnButtonClick(0);
        // // 計算按鈕的高度總和
        moduleRectTransform.sizeDelta += new Vector2(0f, 22*numberOfButtons);//buttonRectTransform.rect.height);
        imgRectTransform.sizeDelta += new Vector2(0f, 22*numberOfButtons);//buttonRectTransform.rect.height);
    }
    // 按鈕點擊事件
    void OnButtonClick(int clickedButtonIndex)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == clickedButtonIndex)
            {
                // 将点击的按钮的interactable设置为false，使其不可交互
                buttons[i].interactable = false;
                // 同时将对应的LaunchOpts中的Selected设置为true
                LaunchOpts[i].Selected = true;
            }
            else
            {
                // 将其他按钮的interactable设置为true，使其恢复正常状态
                buttons[i].interactable = true;
                // 同时将其他按钮对应的LaunchOpts中的Selected设置为false
                LaunchOpts[i].Selected = false;
            }
        }
        scpt_cfg.LaunchUpdate();
    }
    public int getCurrentOpt(){
        for (int i = 0; i < buttons.Length; i++)
        {
            if (!buttons[i].interactable)
            {
                return i;
            }
        }

        // 如果没有按钮被按下，返回-1或者其他合适的值，具体根据需求调整
        return -1;        
    }

    public float getHeight(){
        return 22*numberOfButtons+30;
    }
}
