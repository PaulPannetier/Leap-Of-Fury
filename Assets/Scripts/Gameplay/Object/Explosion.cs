using System;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    private float lastTimeLauch = -10;
    private ToricObject toricObject;

    public ExplosionData explosionData;
    public Action<Collider2D> callbackOnTouch;

    private void Awake()
    {
        callbackOnTouch = (Collider2D arg) => { };
        toricObject = GetComponent<ToricObject>();
    }

    public void Lauch(ExplosionData explosionData)
    {
        this.explosionData = explosionData;
        Invoke(nameof(StartExplode), explosionData.delay);
    }

    public void Lauch()
    {
        Invoke(nameof(StartExplode), explosionData.delay);
    }

    private void StartExplode()
    {
        lastTimeLauch = Time.time;
        ExplosionManager.instance.CreateExplosion((Vector2)transform.position + explosionData.offset, explosionData.force);
    }

    private void Update()
    {
        if(Time.time - lastTimeLauch <= explosionData.duration)
        {
            Collider2D[] cols = PhysicsToric.OverlapCircleAll((Vector2)transform.position + explosionData.offset, explosionData.radius, explosionData.layerMask);
            foreach (Collider2D col in cols)
            {
                OnCollide(col);
            }
        }
        else if(lastTimeLauch > 0f)
        {
            Destroy();
        }
    }

    private void OnCollide(Collider2D collider)
    {
        if(toricObject.isAClone)
        {
            toricObject.original.GetComponent<Explosion>().OnCollide(collider);
            return;
        }
        callbackOnTouch.Invoke(collider);
    }

    public void Destroy()
    {
        toricObject.RemoveClones();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position + explosionData.offset, explosionData.radius);
    }

    [Serializable]
    public struct ExplosionData
    {
        public Vector2 offset;
        public float force;
        public float radius;
        public float delay;
        public float duration;
        public LayerMask layerMask;

        public ExplosionData(in Vector2 offset, float force, float radius, float delay, float duration, LayerMask layerMask)
        {
            this.offset = offset;
            this.force = force;
            this.radius = radius;
            this.delay = delay;
            this.duration = duration;
            this.layerMask = layerMask;
        }

        public ExplosionData Clone() => new ExplosionData(offset, force, radius, delay, duration, layerMask);
    }
}
