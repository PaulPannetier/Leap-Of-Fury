using UnityEngine;

public class IntoTheJungleManager : LevelManager
{
    [Header("Into The Jungle var")]
    [SerializeField] private GameObject playerLightPrefabs;

    protected override void StartLevel()
    {
        base.StartLevel();
        InitIntoTheJungle(true);
    }

    protected override void RestartLevel()
    {
        base.RestartLevel();
        InitIntoTheJungle(false);
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
