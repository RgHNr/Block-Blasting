using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{

    [SerializeField] TMP_Text LevelText , PanelLevelText , CoinsText;
    [SerializeField] Image image;
    [SerializeField] GameObject WinPanel;
    [SerializeField] public Button ContinueButton;
    [SerializeField] public GameObject TutorialPanel;
    [SerializeField] public Image hand;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void UpdateLevelText(int level)
    {
        LevelText.text = "Level " + level.ToString();
        PanelLevelText.text = "Level " + level.ToString();
    }

    public void UpdateProgressBar(int count, int TotalCount)
    {
        image.fillAmount = (float)count / TotalCount;
    }

    public void AcceptButton()
    {
        TutorialManager.Instance.OnActionPerformed(TutorialCondition.ButtonClick);
    }

    

    public void ShowWinPanel()
    {
        WinPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void UpdateCoins(int gold)
    {
        CoinsText.text = gold.ToString() + " Coins";
    }
}
