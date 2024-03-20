using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static LevelManager;

public class FinishMenuPlayerDisplayer : MonoBehaviour
{
    [SerializeField] private Transform charImageUI;
    [SerializeField] private Slider sliderBar;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Init(in PlayerScore playerScore, bool win)
    {
        sliderBar.value = (float)playerScore.nbKills / (float)PlayerScore.nbKillsToWin;
        sliderBar.fillRect.GetComponent<Image>().color = playerScore.playerCommon.color;
        Instantiate(playerScore.playerCommon.charImageUIPrefabs, charImageUI);
        scoreText.text = playerScore.nbKills.ToString();
    }
}
