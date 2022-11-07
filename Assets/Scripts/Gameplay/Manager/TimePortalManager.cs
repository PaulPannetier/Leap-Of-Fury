using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimePortalManager : MonoBehaviour
{
    public static TimePortalManager instance;

    private List<GameObject> lstVisualPortal;

    [SerializeField] private Vector2[] portalSpawnPos;
    [SerializeField] private Vector2[] portalTPPos;
    [SerializeField] private GameObject portalPrefaps;
    [SerializeField] private Component[] componentsToDestroyInPortalVisual;

    [SerializeField] private float minDurationBetweenPortals;
    [SerializeField] private float maxDurationBetweenPortals;
    [SerializeField, Tooltip("Time waiting when match begin")] private float beginSleepTime = 5f;

    [HideInInspector] public bool isLastPortalActivated = false;

    private void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("An other instance of TimePortalManager are create!");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(UpdateCorout());
        EventManager.instance.callbackOnLevelRestart += OnLevelRestart;
    }

    private void OnLevelRestart(string levelName)
    {
        StopCoroutine(UpdateCorout());
        StartCoroutine(UpdateCorout());
    }

    private IEnumerator UpdateCorout()
    {
        yield return Useful.GetWaitForSeconds(beginSleepTime);

        while(true)
        {
            float duration = Random.Rand(minDurationBetweenPortals, maxDurationBetweenPortals);
            yield return new WaitForSeconds(duration);

            //Spawn a portal
            print("Spawning Portal!");
            GameObject portalGO = Instantiate(portalPrefaps, portalSpawnPos.GetRandom(), Quaternion.identity, CloneParent.cloneParent);
            isLastPortalActivated = true;
            while (isLastPortalActivated)
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// Instantiate all of the visual of the portal were the charIn the main portal can tp to
    /// </summary>
    /// <returns>The list contain all of the portals visual</returns>
    public List<GameObject> CreateVisualTPPortal()
    {
        lstVisualPortal = new List<GameObject>();
        foreach (Vector2 p in portalTPPos)
        {
            GameObject portalGO = Instantiate(portalPrefaps, p, Quaternion.identity, CloneParent.cloneParent);
            foreach (Component componentType in componentsToDestroyInPortalVisual)
            {
                if(portalGO.TryGetComponent(componentType.GetType(), out Component c))
                {
                    Destroy(c);
                }
            }
            lstVisualPortal.Add(portalGO);
        }
        return lstVisualPortal;
    }

    public GameObject ActivateTpPortal(int index)
    {
        GameObject portalGO = Instantiate(portalPrefaps, lstVisualPortal[index].transform.position, Quaternion.identity, CloneParent.cloneParent);
        portalGO.GetComponent<TimePortal>().tpChar = true;

        for(int i = lstVisualPortal.Count - 1; i >= 0; i--)
        {
            Destroy(lstVisualPortal[i]);
        }

        lstVisualPortal.Clear();
        return portalGO;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if(portalSpawnPos != null)
        {
            foreach (Vector2 pos in portalSpawnPos)
            {
                Circle.GizmosDraw(pos, 0.4f);
            }
        }

        Gizmos.color = Color.red;
        if (portalTPPos != null)
        {
            foreach (Vector2 pos in portalTPPos)
            {
                Circle.GizmosDraw(pos, 0.4f);
            }
        }
    }

    private void OnValidate()
    {
        minDurationBetweenPortals = Mathf.Max(0f, minDurationBetweenPortals);
        maxDurationBetweenPortals = Mathf.Max(0f, maxDurationBetweenPortals);
    }
}
