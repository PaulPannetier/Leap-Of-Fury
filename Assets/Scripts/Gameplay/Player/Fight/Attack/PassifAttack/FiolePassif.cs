using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class FiolePassif : PassifAttack
{
    private bool _isSpawning = false;
    public bool isSpawning
    {
        get => _isSpawning;
        set
        {
            if (!isSpawning && value)
            {
                StartCoroutine(GenerateFiole());
            }
            else if (isSpawning && !value)
            {
                StopCoroutine(GenerateFiole());
            }
            _isSpawning = value;
        }
    }

    private List<Fiole> fioles;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    [SerializeField] private float spawnDuration = 15f, spawnWait = 10f;
    [SerializeField] private GameObject fiolePrefabs;
    [SerializeField] private Vector2[] spawnPoints;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        fioles = new List<Fiole>();
        isSpawning = true;
    }

    private void SpawnFiole()
    {
        Vector2 pos = spawnPoints[Random.RandExclude(0, spawnPoints.Length)];
        GameObject fiole = Instantiate(fiolePrefabs, pos, Quaternion.identity, CloneParent.cloneParent);
        Fiole fioleComp = fiole.GetComponent<Fiole>();
        fioleComp.playerCommon = playerCommon;
        ClearFioles();
        fioles.Add(fioleComp);
    }

    private void ClearFioles()
    {
        foreach (Fiole f in fioles)
        {
            if(f != null)
                f.Destroy();
        }
        fioles.Clear();
    }

    private IEnumerator GenerateFiole()
    {
        while (!enableBehaviour)
            yield return null;

        WaitForSeconds waitTime = new WaitForSeconds(spawnWait);
        while (!enableBehaviour)
            yield return null;
        WaitForSeconds duration = new WaitForSeconds(spawnDuration);
        while (true)
        {
            if(enableBehaviour)
            {
                ClearFioles();
                yield return waitTime;
                SpawnFiole();
                yield return duration;
            }
            else
            {
                yield return null;
            }
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        Gizmos.color = Color.red;
        foreach (Vector2 pos in spawnPoints)
        {
            Gizmos.DrawWireSphere(pos, 0.4f);
        }
    }

#endif
}
