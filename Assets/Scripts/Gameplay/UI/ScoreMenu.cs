using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class ScoreMenu : MonoBehaviour
{
    private LevelManager.PlayerScore[] playersScore;
    private GameObject[,] skulls;
    private GameObject[] scoreDisplayerLines;
    private Image panelBG;
    private Action callbackEnd;

    [Header("Info")]
    [SerializeField] private GameObject skullPrefabs;
    [SerializeField] private GameObject scoreDisplayerLinePrefabs;
    [SerializeField] private Transform scoreDisplayerLinesParents;

    [Header("Settings")]
    [SerializeField] private Color skullInactivatedColor;
    [SerializeField] private Color skullActivatedColor;
    [SerializeField] private float durationBeforeFade = 2f, durationAfterFade = 2f;
    [SerializeField] private float fadeGapDuration = 0.3f, fadingDuration = 0.5f;
    
    private void Awake()
    {
        panelBG = GetComponent<Image>();
        panelBG.enabled = false;
    }

    public void DisplayScoreMenu(LevelManager.PlayerScore[] playersScore, Action callbackEnd)
    {
        this.callbackEnd = callbackEnd;
        this.playersScore = playersScore;
        panelBG.enabled = true;
        InstanciateVisual();
        StartCoroutine(FadeSkull());
    }

    private void DisableScoreMenu()
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
        panelBG.enabled = false;
        callbackEnd.Invoke();
    }

    private IEnumerator FadeSkull()
    {
        yield return Useful.GetWaitForSeconds(durationBeforeFade);

        int maxSkull = 0;
        for (int i = 0; i < playersScore.Length; i++)
        {
            maxSkull = Mathf.Max(maxSkull, playersScore[i].nbKills);
        }

        int skullIndex = 0;
        while(skullIndex < maxSkull)
        {
            for (int i = 0; i < skulls.GetLength(0); i++)
            {
                if(playersScore[i].nbKills > skullIndex)
                {
                    StartCoroutine(ActivateSkull(i, skullIndex, fadingDuration));
                }
            }
            skullIndex++;
            yield return Useful.GetWaitForSeconds(fadeGapDuration);
        }

        yield return Useful.GetWaitForSeconds(durationAfterFade);

        DisableScoreMenu();
    }

    private IEnumerator ActivateSkull(int i, int j, float duration)
    {
        Image img = skulls[i, j].GetComponent<Image>();

        float time = Time.time;
        while (Time.time - time < duration)
        {
            img.color = Color.Lerp(Color.white, skullActivatedColor, (Time.time - time) / duration);
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

    #region OnValidate

    #if UNITY_EDITOR

    private void OnValidate()
    {
        fadeGapDuration = Mathf.Max(fadeGapDuration, 0f);
        fadingDuration = Mathf.Max(fadingDuration, 0f);
        durationBeforeFade = Mathf.Max(durationBeforeFade, 0f);
        durationAfterFade = Mathf.Max(durationAfterFade, 0f);
    }

    #endif

    #endregion
}
