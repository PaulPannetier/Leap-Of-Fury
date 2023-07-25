using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class ScoreMenu : MonoBehaviour
{
    private LevelManager.PlayerScore[] playersScore;
    private GameObject[,] skulls;
    private GameObject[] scoreDisplayerLines;
    private Image panelBG;

    [SerializeField] private GameObject skullPrefabs;
    [SerializeField] private GameObject scoreDisplayerLinePrefabs;
    [SerializeField] private Transform scoreDisplayerLinesParents;
    [SerializeField] private Color skullActivatedColor;
    [SerializeField, Range(0.01f, 0.5f)] private float fadeTimeOffsetPercent = 0.15f;

    [field:SerializeField] public float scoreMenuDuration { get; private set; } = 5f;

    private void Awake()
    {
        panelBG = GetComponent<Image>();
        panelBG.enabled = false;
    }

    public void DisplayScoreMenu(LevelManager.PlayerScore[] playersScore)
    {
        this.playersScore = playersScore;
        panelBG.enabled = true;
        InstanciateVisual();
        StartCoroutine(FadeSkull(scoreMenuDuration));
        StartCoroutine(DisableScoreMenu(scoreMenuDuration));
    }

    private IEnumerator DisableScoreMenu(float duration)
    {
        yield return Useful.GetWaitForSeconds(duration);
        if (skulls != null)
        {
            for (int i = 0; i < skulls.GetLength(0); i++)
            {
                for (int j = 0; j < skulls.GetLength(1); j++)
                {
                    Destroy(skulls[i, j]);
                }
            }
        }
        if (scoreDisplayerLines != null)
        {
            for (int i = 0; i < scoreDisplayerLines.Length; i++)
            {
                Destroy(scoreDisplayerLines[i]);
            }
        }
        skulls = null;
        scoreDisplayerLines = null;
        panelBG.enabled = false;
    }

    private IEnumerator FadeSkull(float duration)
    {
        yield return Useful.GetWaitForSeconds(fadeTimeOffsetPercent * duration);

        int maxSkull = 0;
        for (int i = 0; i < playersScore.Length; i++)
        {
            maxSkull = Mathf.Max(maxSkull, playersScore[i].nbKills);
        }
        float stepTime = (duration * (1f - (2f * fadeTimeOffsetPercent)))  / maxSkull;
        int skullIndex = 0;

        while(skullIndex < maxSkull)
        {
            for (int i = 0; i < skulls.GetLength(0); i++)
            {
                if(playersScore[i].nbKills > skullIndex)
                {
                    StartCoroutine(ActivateSkull(i, skullIndex, stepTime * 0.8f));
                }
            }
            skullIndex++;
            yield return Useful.GetWaitForSeconds(stepTime);
        }
    }

    private IEnumerator ActivateSkull(int i, int j, float t)
    {
        Image img = skulls[i, j].GetComponent<Image>();
        Animator anim = skulls[i, j].GetComponent<Animator>();
        anim.CrossFade("Active", 0f);

        float time = Time.time;

        while (Time.time - time < t)
        {
            img.color = Color.Lerp(Color.white, skullActivatedColor, (Time.realtimeSinceStartup - time) / t);
            yield return null;
        }
    }

    private void InstanciateVisual()
    {
        scoreDisplayerLines = new GameObject[playersScore.Length];
        skulls = new GameObject[playersScore.Length, LevelManager.PlayerScore.nbKillsToWin];
        for (int i = 0; i < playersScore.Length; i++)
        {
            scoreDisplayerLines[i] = Instantiate(scoreDisplayerLinePrefabs, scoreDisplayerLinesParents);
            for (int j = 0; j < LevelManager.PlayerScore.nbKillsToWin; j++)
            {
                skulls[i, j] = Instantiate(skullPrefabs, scoreDisplayerLines[i].transform);
            }
        }
    }
}
