using UnityEngine;
using TMPro;

public class StartLevelManager : MonoBehaviour
{
    private GameObject levelNameUI;
    private GameObject startLevelAnim;

    private void Awake()
    {
        levelNameUI = transform.GetChild(0).gameObject;
        startLevelAnim = transform.GetChild(1).gameObject;
    }

    private void Start()
    {
        EventManager.instance.callbackOnLevelStart += LevelStart;
        EventManager.instance.callbackOnLevelRestart += LevelRestart;
    }

    private void LevelStart(string levelName)
    {
        levelNameUI.SetActive(true);
        levelNameUI.GetComponent<TextMeshProUGUI>().text = levelName.ToUpper();
        Animator levelNameAnim = levelNameUI.GetComponent<Animator>();
        if(levelNameAnim.GetAnimationLength("StartLevel", out float length))
        {
            levelNameAnim.CrossFade(Animator.StringToHash("StartLevel"), 0, 0);
            this.Invoke(nameof(DisableGO),levelNameUI, length);
            Invoke(nameof(StartBegLevelAnim), length);
        }
        else
        {
            Debug.LogWarning("No animation StartLevel found");
        }
    }

    private void DisableGO(GameObject go)
    {
        go.SetActive(false);
    }

    private void LevelRestart(string levelName)
    {
        StartBegLevelAnim();
    }

    private void StartBegLevelAnim()
    {
        startLevelAnim.SetActive(true);
        Animator anim = startLevelAnim.GetComponent<Animator>();
        if(anim.GetAnimationLength("RestartLevel", out float length))
        {
            anim.CrossFade(Animator.StringToHash("RestartLevel"), 0, 0);
            this.Invoke(nameof(DisableGO), startLevelAnim, length);
        }
        else
        {
            Debug.LogWarning("No animation RestartLevel found");
        }
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelStart -= LevelStart;
        EventManager.instance.callbackOnLevelRestart -= LevelRestart;
    }
}
