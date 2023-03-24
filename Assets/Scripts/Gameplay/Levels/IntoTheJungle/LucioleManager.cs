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
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        CreateLuciole(lucioleCount);
        EventManager.instance.callbackOnLevelRestart += Restart;
        PauseManager.instance.callBackOnPauseDisable += DisableLucioles;
        PauseManager.instance.callBackOnPauseEnable += EnableLucioles;
    }

    private void Restart(string levelName)
    {
        foreach(Luciole luciole in lstLucioles)
        {
            Destroy(luciole.gameObject);
        }
        lstLucioles.Clear();

        StartCoroutine(CreateLucioleIn2FrameAfter());
    }

    private IEnumerator CreateLucioleIn2FrameAfter()
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
            Vector2 pos = Random.PointInRectangle(Vector2.zero, PhysicsToric.cameraSize);
            GameObject lucioleGO = Instantiate(luciolePrefab, pos, Quaternion.Euler(0f, 0f, Random.RandExclude(0f, 360f)), go.transform);
        }
    }

    private void DisableLucioles()
    {
        foreach (Luciole l in lstLucioles)
        {
            l.enableBehaviour = false;
        }
    }

    private void EnableLucioles()
    {
        foreach (Luciole l in lstLucioles)
        {
            l.enableBehaviour = true;
        }
    }
}
