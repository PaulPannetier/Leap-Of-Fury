using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class LucioleManager : MonoBehaviour
{
    public static LucioleManager instance;

    [HideInInspector] public List<Luciole> lstLucioles = new List<Luciole>();

    [SerializeField] private GameObject luciolePrefab;
    [SerializeField] private int lucioleCount = 10;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("Another instance of LucioleManager is create in the scene");
            Destroy(this);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if(!CycleDayNightManager.instance.isDay)
        {
            CreateLuciole(lucioleCount);
        }

        EventManager.instance.callbackOnLevelStart += OnLevelStart;
        EventManager.instance.callbackOnLevelRestart += OnLevelRestart;
        PauseManager.instance.callBackOnPauseDisable += EnableLucioles;
        PauseManager.instance.callBackOnPauseEnable += DisableLucioles;
    }

    private void OnLevelStart(string levelName)
    {
        DestroyLucioles();
        StartCoroutine(CreateLucioleIn2Frames());
    }

    private void OnLevelRestart(string levelName)
    {
        DestroyLucioles();
        StartCoroutine(CreateLucioleIn2Frames());
    }

    private void DestroyLucioles()
    {
        foreach (Luciole luciole in lstLucioles)
        {
            Destroy(luciole.gameObject);
        }
        lstLucioles.Clear();
    }

    private IEnumerator CreateLucioleIn2Frames()
    {
        yield return null;
        yield return null;
        if (!CycleDayNightManager.instance.isDay)
        {
            CreateLuciole(lucioleCount);
        }
    }

    private void CreateLuciole(int count)
    {
        GameObject go = new GameObject("Luciole parent");
        go.transform.parent = CloneParent.cloneParent;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = Random.PointInRectangle(Vector2.zero, LevelMapData.currentMap.mapSize);
            GameObject lucioleGO = Instantiate(luciolePrefab, pos, Quaternion.Euler(0f, 0f, Random.RandExclude(0f, 360f)), go.transform);
        }
    }

    private void DisableLucioles()
    {
        foreach (Luciole luciole in lstLucioles)
        {
            luciole.enableBehaviour = false;
        }
    }

    private void EnableLucioles()
    {
        foreach (Luciole luciole in lstLucioles)
        {
            luciole.enableBehaviour = true;
        }
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        PauseManager.instance.callBackOnPauseEnable -= DisableLucioles;
        PauseManager.instance.callBackOnPauseDisable -= EnableLucioles;
        EventManager.instance.callbackOnLevelStart -= OnLevelStart;
        EventManager.instance.callbackOnLevelRestart -= OnLevelRestart;
    }
}
