using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabControl : MonoBehaviour
{
    public Button manualTab; 
    public Button generateTab;
    public GameObject manualPanel;
    public GameObject generatePanel;

    // Start is called before the first frame update
    void Start()
    {
        manualTab.onClick.AddListener(ToggleManual);
        generateTab.onClick.AddListener(ToggleGenerate);        

        // 初始時顯示manual panel，generate panel隱藏
        manualPanel.SetActive(true);
        generatePanel.SetActive(false);
    }

    void ToggleManual()
    {
        // 顯示manual panel，隱藏generate panel
        manualPanel.SetActive(true);
        generatePanel.SetActive(false);

        // 設定manual tab為正常顏色，generate tab為灰色
        manualTab.interactable = false;
        generateTab.interactable = true;
    }

    void ToggleGenerate()
    {
        // 顯示generate panel，隱藏manual panel
        manualPanel.SetActive(false);
        generatePanel.SetActive(true);

        // 設定generate tab為正常顏色，manual tab為灰色
        manualTab.interactable = true;
        generateTab.interactable = false;
    }
}
