using System.Collections;
using UnityEngine;

public class FogManager : MonoBehaviour
{
    private static FogManager instance;

    private GameObject currentFog;

    [SerializeField] private Vector2[] spawnPoints;
    [SerializeField] private float minDuration, maxDuration;
    [SerializeField] private float minWaitDuration, maxWaitDuration;
    [SerializeField] private GameObject fogPrefab;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(GenerateFog());
    }

    private IEnumerator GenerateFog()
    {
        while(true)
        {
            float waitingTime = Random.Rand(minWaitDuration, maxWaitDuration);
            yield return new WaitForSeconds(waitingTime);
            currentFog = Instantiate(fogPrefab, spawnPoints.GetRandom(), Quaternion.identity, CloneParent.cloneParent);
            waitingTime = Random.Rand(minDuration, maxDuration);
            yield return new WaitForSeconds(waitingTime);
            currentFog.GetComponent<Fog>().Destroy();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (Vector2 point in spawnPoints)
        {
            Gizmos.DrawWireSphere(point, 0.5f);
        }
    }
}
