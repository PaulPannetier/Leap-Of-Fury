using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FinishMenuPlayerDisplayer : MonoBehaviour
{
    [SerializeField] private Transform charImageUI;
    [SerializeField] private Slider sliderBar;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Init(in LevelManager.PlayerScore playerScore, bool win)
    {
        sliderBar.value = (float)playerScore.nbKills / (float)LevelManager.PlayerScore.nbKillsToWin;
        sliderBar.fillRect.GetComponent<Image>().color = playerScore.playerCommon.color;
        Instantiate(playerScore.playerCommon.charImageUIPrefabs, charImageUI);
        scoreText.text = playerScore.nbKills.ToString();
    }
}
