using UnityEngine;
using UnityEngine.UI;
using static EndMenuPlayerDisplayer;

public class EndLevelMenu : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup layoutGroup;
    [SerializeField] private GameObject playerMenuDisplayerPrefabs;
    [SerializeField] private float displayDuration = 3f;

    private void Awake()
    {
        EventManager.instance.callbackOnLevelEnd += OnLevelEnd;
    }

    private void OnLevelEnd(LevelManager.EndLevelData endLevelData)
    {
        for (int i = 0; i < endLevelData.playersScore.Length; i++)
        {
            LevelManager.PlayerScore playerScore = endLevelData.playersScore[i];

            DisplaySettings displaySettings = new DisplaySettings(LevelManager.PlayerScore.nbKillsToWin, playerScore.nbKills, playerScore.playerCommon.charImageUIPrefabs, displayDuration);
            GameObject psDisplayer = Instantiate(playerMenuDisplayerPrefabs, layoutGroup.transform);
            EndMenuPlayerDisplayer displayer = psDisplayer.GetComponent<EndMenuPlayerDisplayer>();
            displayer.Display(displaySettings);
        }
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnLevelEnd -= OnLevelEnd;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        displayDuration = Mathf.Max(displayDuration, 0f);
    }

#endif
}
