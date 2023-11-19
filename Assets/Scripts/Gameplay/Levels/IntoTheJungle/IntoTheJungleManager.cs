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

        GameObject[] chars = GameObject.FindGameObjectsWithTag("Char");
        foreach (GameObject charGO in chars)
        {
            GameObject lightTouchFloor = Instantiate(playerLightPrefabs, charGO.transform.position, Quaternion.identity, charGO.transform);
            lightTouchFloor.transform.localPosition = Vector3.zero;
        }
    }
}
