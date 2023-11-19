using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;

public class Bomb : MonoBehaviour
{
    private Attack attacklauncher;
    private bool isExplosing = false;
    private List<uint> idAlreadyTouch;
    private Animator anim;
    private ToricObject toricObject;

    public bool enableBehaviour = true;

    [SerializeField] private float explosionDelay = 1f;
    [SerializeField] private float explosionRange = 3f;
    [SerializeField] private float explosionDuration = 0.5f;
    [SerializeField] private float shockWaveForce = 7f;
    [SerializeField] private LayerMask charMask;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        toricObject = GetComponent<ToricObject>();
    }

    private void Start()
    {
        idAlreadyTouch = new List<uint>();
        isExplosing = false;
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    public void Lauch(Attack launcher)
    {
        this.attacklauncher = launcher;
        StartCoroutine(ExplodeCorout());
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        if (toricObject.isAClone)
            return;

        if (!isExplosing)
            return;

        Collider2D[] cols = PhysicsToric.OverlapCircleAll(transform.position, explosionRange, charMask);
        foreach (Collider2D col in cols)
        {
            if (col.CompareTag("Char"))
            {
                GameObject charHit = col.GetComponent<ToricObject>().original;
                PlayerCommon pc = charHit.GetComponent<PlayerCommon>();
                if(!idAlreadyTouch.Contains(pc.id))
                {
                    attacklauncher.OnTouchEnemy(charHit);
                    idAlreadyTouch.Add(pc.id);
                }
            }
        }
    }

    private IEnumerator ExplodeCorout()
    {
        float oldTime = Time.time;
        float timeCount = 0f;
        while (true)
        {
            yield return null;
            if (enableBehaviour)
            {
                timeCount += Time.time - oldTime;
                oldTime = Time.time;

                if (timeCount > explosionDelay)
                {
                    Explode();
                    break;
                }
            }
        }
    }

    private IEnumerator DestroyCorout()
    {
        float oldTime = Time.time;
        float timeCount = 0f;
        while (true)
        {
            yield return null;
            if (enableBehaviour)
            {
                timeCount += Time.time - oldTime;
                oldTime = Time.time;

                if (timeCount > explosionDuration)
                {
                    Destroy();
                    break;
                }
            }
        }
    }

    private void Destroy()
    {
        toricObject.RemoveClones();
        Destroy(gameObject);
    }

    private void Explode()
    {
        isExplosing = true;
        anim.SetTrigger("Explode");
        ExplosionManager.instance.CreateExplosion(transform.position, shockWaveForce);
        StartCoroutine(DestroyCorout());
    }

    #region Gizmos/OnValidate

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw(transform.position, explosionRange);
    }

    private void OnValidate()
    {
        explosionDelay = Mathf.Max(0f, explosionDelay);
        explosionRange = Mathf.Max(0f, explosionRange);
    }

    #endregion
}
