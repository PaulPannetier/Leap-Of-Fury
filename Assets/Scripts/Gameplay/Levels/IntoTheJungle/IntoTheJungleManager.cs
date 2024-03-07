using UnityEngine;
using System.Collections;

public class IntoTheJungleManager : LevelManager
{
    [Header("Into The Jungle var")]
    [SerializeField] private GameObject playerLightPrefabs;

    protected override void StartLevel()
    {
        base.StartLevel();
        StartCoroutine(InitIntoTheJungleCorout(true));
    }

    protected override void RestartLevel()
    {
        base.RestartLevel();
        StartCoroutine(InitIntoTheJungleCorout(false));
    }

    private IEnumerator InitIntoTheJungleCorout(bool start)
    {
        yield return null;
        yield return null;

        InitIntoTheJungle(start);
    }

    private void InitIntoTheJungle(bool start)
    {
        if ((start && CycleDayNightManager.instance.startLevelAtDay) || CycleDayNightManager.instance.isDay)
            return;

        foreach (Transform t in charParent)
        {
            GameObject lightTouchFloor = Instantiate(playerLightPrefabs, t.position, Quaternion.identity, t.transform);
            lightTouchFloor.transform.localPosition = Vector3.zero;
        }
    }
}
