using DG.Tweening;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private TutorialCondition currentCondition;
    private int actionCounter = 0;

    private void Awake() => Instance = this;

    public void Init(LevelData levelData)
    {
        if (!levelData.hasTutorial)
        {
           if(UIManager.Instance.TutorialPanel!=null)UIManager.Instance.TutorialPanel?.SetActive(false);
           return;
        }

        currentCondition = levelData.dismissCondition;
        actionCounter = 0;
        if (UIManager.Instance.hand != null) UIManager.Instance.hand.gameObject.transform.DORotate(new Vector3(0, 0, -127), 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo);
        // Setup UI
        UIManager.Instance.TutorialPanel.SetActive(true);
        // If you have a text component: UIManager.Instance.TutorialText.text = levelData.tutorialMessage;
    }

    public void OnActionPerformed(TutorialCondition actionType)
    {
        if (currentCondition == TutorialCondition.None) return;

        if (actionType == TutorialCondition.SingleClick && currentCondition == TutorialCondition.SingleClick)
        {
            CompleteTutorial();
        }
        else if (actionType == TutorialCondition.SingleClick && currentCondition == TutorialCondition.ThreeClicks)
        {
            actionCounter++;
            if (actionCounter >= 3) CompleteTutorial();
        }
        else if (actionType==TutorialCondition.ButtonClick && currentCondition == TutorialCondition.ButtonClick)
        {
            CompleteTutorial(); 
        }
        else if (actionType == TutorialCondition.FirstFuse && currentCondition == TutorialCondition.FirstFuse)
        {
            CompleteTutorial();
        }
    }

    private void CompleteTutorial()
    {
        UIManager.Instance.TutorialPanel.SetActive(false);
        currentCondition = TutorialCondition.None;
    }
}