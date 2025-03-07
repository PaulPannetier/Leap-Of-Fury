using System;
using UnityEngine;
using Collision2D;
using System.Collections;
using System.Collections.Generic;

public class Explosion : MonoBehaviour
{
    private float lastTimeLauch = -10;
    protected SpriteRenderer spriteRenderer;
    private ToricObject toricObject;
    private bool isExploding = false;
    private List<UnityEngine.Collider2D> colAlreadyTouch;

    protected bool _enableBehaviour;
    public virtual bool enableBehaviour
    {
        get => _enableBehaviour;
        set => _enableBehaviour = value;
    }
    public bool oneTouchPerCollider;
    public ExplosionData explosionData;
    public Action<UnityEngine.Collider2D> callbackOnTouch;
    public Action<Explosion> callbackOnDestroy;

    protected virtual void Awake()
    {
        callbackOnTouch = (UnityEngine.Collider2D arg) => { };
        callbackOnDestroy = (Explosion arg) => { };
        toricObject = GetComponent<ToricObject>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        colAlreadyTouch = new List<UnityEngine.Collider2D>();
    }

    public virtual void Launch(ExplosionData explosionData)
    {
        this.explosionData = explosionData;
        StartCoroutine(InvokeWithPause(StartExplode, explosionData.delay));
    }

    public virtual void Launch()
    {
        StartCoroutine(InvokeWithPause(StartExplode, explosionData.delay));
    }

    private IEnumerator InvokeWithPause(Action method, float delay)
    {
        yield return PauseManager.instance.Wait(delay);

        method.Invoke();
    }

    private void StartExplode()
    {
        toricObject.boundsSize = new Vector2(explosionData.radius, explosionData.radius);
        lastTimeLauch = Time.time;
        isExploding = true;
        ExplosionManager.instance.CreateExplosion((Vector2)transform.position + explosionData.offset, explosionData.force);
        colAlreadyTouch.Clear();
    }

    protected virtual void Update()
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
                if(oneTouchPerCollider)
                {
                    if(!colAlreadyTouch.Contains(col))
                    {
                        colAlreadyTouch.Add(col);
                        OnCollide(col);
                    }
                }
                else
                {
                    colAlreadyTouch.Add(col);
                    OnCollide(col);
                }
            }
        }

        if(isExploding && Time.time - lastTimeLauch > explosionData.duration)
        {
            Destroy();
        }
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
