using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    public static ExplosionManager instance { get; private set; }

    public static void AddMovingWithExplosionRidgidbody(Rigidbody2D rb)
    {
        instance.AddRidgidbody(rb);
    }

    private List<Rigidbody2D> moveWidthExplosion;
    private float maxDistance => Mathf.Max(LevelMapData.currentMap.mapSize.x, LevelMapData.currentMap.mapSize.y) * 0.5f;

    [SerializeField] private AnimationCurve explosionForceCurvePerDistance;

    private void Awake()
    {
        instance = this;
        moveWidthExplosion = new List<Rigidbody2D>();
    }

    private void AddRidgidbody(Rigidbody2D rb)
    {
        moveWidthExplosion.Add(rb);
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
