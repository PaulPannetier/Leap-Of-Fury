using UnityEngine;
using TMPro;

public class StartLevelManager : MonoBehaviour
{
    public bool enableBehaviour = true;
    [SerializeField] private GameObject levelNameUI;

    private void Start()
    {
        EventManager.instance.callbackOnLevelStart += LevelStart;
        EventManager.instance.callbackOnLevelRestart += LevelRestart;
    }

    private void LevelStart(string levelName)
    {
        if(!enableBehaviour)
        {
            levelNameUI.SetActive(false);
            return;
        }

        levelNameUI.SetActive(true);
        levelNameUI.GetComponent<TextMeshProUGUI>().text = levelName.ToUpper();
        Animator levelNameAnim = levelNameUI.GetComponent<Animator>();

        AnimationClip animClips = levelNameAnim.GetAnimationsClips()[0];
        levelNameAnim.CrossFade(animClips.name, 0, 0);
        this.Invoke(DisableGO, levelNameUI, animClips.length);
    }

    private void DisableGO(GameObject go)
    {
        go.SetActive(false);
    }

    private void LevelRestart(string levelName)
    {
        
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelStart -= LevelStart;
        EventManager.instance.callbackOnLevelRestart -= LevelRestart;
    }
}
