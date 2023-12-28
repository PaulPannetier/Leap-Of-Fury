using System;
using UnityEngine;
using Collision2D;
using System.Collections;

public class Explosion : MonoBehaviour
{
    private float lastTimeLauch = -10;
    private SpriteRenderer spriteRenderer;
    private ToricObject toricObject;
    private bool isExploding = false;

    public bool enableBehaviour = true;
    public ExplosionData explosionData;
    public Action<UnityEngine.Collider2D> callbackOnTouch;
    public Action<Explosion> callbackOnDestroy;

    private void Awake()
    {
        callbackOnTouch = (UnityEngine.Collider2D arg) => { };
        callbackOnDestroy = (Explosion arg) => { };
        toricObject = GetComponent<ToricObject>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Launch(ExplosionData explosionData)
    {
        this.explosionData = explosionData;
        StartCoroutine(InvokeWithPause(StartExplode, explosionData.delay));
    }

    public void Launch()
    {
        StartCoroutine(InvokeWithPause(StartExplode, explosionData.delay));
    }

    private IEnumerator InvokeWithPause(Action method, float delay)
    {
        float timeCounter = 0f;
        while (timeCounter < delay)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }
        method.Invoke();
    }

    private void StartExplode()
    {
        toricObject.bounds = new Bounds(Vector3.zero, new Vector3(explosionData.radius, explosionData.radius, 0.01f));
        lastTimeLauch = Time.time;
        isExploding = true;
        ExplosionManager.instance.CreateExplosion((Vector2)transform.position + explosionData.offset, explosionData.force);
    }

    private void Update()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimeLauch += Time.deltaTime;
            return;
        }

        if(enableBehaviour && Time.time - lastTimeLauch <= explosionData.duration)
        {
            UnityEngine.Collider2D[] cols = PhysicsToric.OverlapCircleAll((Vector2)transform.position + explosionData.offset, explosionData.radius, explosionData.layerMask);
            foreach (UnityEngine.Collider2D col in cols)
            {
                OnCollide(col);
            }
        }

        if(isExploding && Time.time - lastTimeLauch > explosionData.duration)
        {
            Destroy();
        }
    }

    public void SetColor(in Color color)
    {
        spriteRenderer.color = color;
    }

    private void OnCollide(UnityEngine.Collider2D collider)
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
        callbackOnDestroy.Invoke(this);
        toricObject.RemoveClones();
        Destroy(gameObject);
    }

    #region Gizmos

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position + explosionData.offset, explosionData.radius);
    }

#endif

    #endregion

    #region Custom Struct

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

#endregion
}
