using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class ScoreMenu : MonoBehaviour
{
    private PlayerScore[] playersScore;
    private GameObject[,] skulls;
    private GameObject[] scoreDisplayerLines;
    private Image panelBG;

    [SerializeField] private GameObject skullPrefabs;
    [SerializeField] private GameObject scoreDisplayerLinePrefabs;
    [SerializeField] private Transform scoreDisplayerLinesParents;
    [SerializeField] private Color skullActivatedColor;

    private void Awake()
    {
        panelBG = GetComponent<Image>();
        panelBG.enabled = false;
    }

    public void SetPlayersKills(in PlayerScore[] playersScore, in float duration)
    {
        this.playersScore = playersScore;
        StartCoroutine(UpdateVisual(duration));
    }

    private IEnumerator UpdateVisual(float duration)
    {
         CreateVisual();
        panelBG.enabled = true;

        int maxSkull = 0;
        for (int i = 0; i < playersScore.Length; i++)
        {
            maxSkull = Mathf.Max(maxSkull, playersScore[i].nbKills);
        }
        float stepTime = duration / maxSkull;
        int skullIndex = 0;

        while(skullIndex < maxSkull)
        {
            yield return new WaitForSecondsRealtime(stepTime);

            for (int i = 0; i < skulls.GetLength(0); i++)
            {
                if(playersScore[i].nbKills > skullIndex)
                {
                    StartCoroutine(ActivateSkull(i, skullIndex, stepTime * 0.8f));
                }
            }
            skullIndex++;
        }
    }

    private IEnumerator ActivateSkull(int i, int j, float t)
    {
        Image img = skulls[i, j].GetComponent<Image>();
        float time = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - time < t)
        {
            img.color = Color.Lerp(Color.white, skullActivatedColor, (Time.realtimeSinceStartup - time) / t);
            yield return null;
        }
    }

    private void CreateVisual()
    {
        scoreDisplayerLines = new GameObject[playersScore.Length];
        skulls = new GameObject[playersScore.Length, PlayerScore.nbKillsToWin];
        if (skulls != null)
        {
            for (int i = 0; i < skulls.GetLength(0); i++)
            {
                scoreDisplayerLines[i] = Instantiate(scoreDisplayerLinePrefabs, scoreDisplayerLinesParents);
                for (int j = 0; j < skulls.GetLength(1); j++)
                {
                    skulls[i, j] = Instantiate(skullPrefabs, scoreDisplayerLines[i].transform);
                }
            }
        }
    }

    private void RemoveVisual()
    {
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
    }

    public void DisableVisual()
    {
        StopCoroutine(nameof(UpdateVisual));
        RemoveVisual();
        panelBG.enabled = false;
    }
}
