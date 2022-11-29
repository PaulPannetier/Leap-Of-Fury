using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager instance { get; private set; }

    private Rigidbody2D[] moveWidthExplosion;
    private float maxDistance;

    [SerializeField] private AnimationCurve explosionForceCurvePerDistance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        ResearchMoveWithExplosionGameobject();
    }

    private void Start()
    {
        maxDistance = Mathf.Max(PhysicsToric.cameraSize.x, PhysicsToric.cameraSize.y) * 0.5f;
    }

    public void ResearchMoveWithExplosionGameobject()
    {
        List<Rigidbody2D> movers = new List<Rigidbody2D>();
        MoveWhenMoverPassThrough[] tmp = FindObjectsOfType<MoveWhenMoverPassThrough>();
        for (int i = 0; i < tmp.Length; i++)
        {
            if (tmp[i].moveOnExplosion)
            {
                Rigidbody2D rb = tmp[i].GetComponent<Rigidbody2D>();
                if (rb != null)
                    movers.Add(rb);
            }
        }
        moveWidthExplosion = movers.ToArray();
    }

    public void CreateExplosion(in Vector2 position, float force)
    {
        Vector2 dir;
        float dist;
        foreach (Rigidbody2D rb in moveWidthExplosion)
        {
            if(rb != null)
            {
                dir = PhysicsToric.Direction(position, rb.transform.position);
                dist = PhysicsToric.Distance(position, rb.transform.position);
                rb.AddForce(dir * (explosionForceCurvePerDistance.Evaluate(Mathf.Clamp01(dist / maxDistance)) * force), ForceMode2D.Impulse);
            }
        }
    }
}
