using System.Collections.Generic;
using UnityEngine;

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
        PauseManager.instance.callBackOnPauseDisable += Disable;
        PauseManager.instance.callBackOnPauseEnable += Enable;
    }

    public void Lauch(Attack launcher)
    {
        this.attacklauncher = launcher;
        Invoke(nameof(Explode), explosionDelay);
    }

    private void Update()
    {
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

    private void Explode()
    {
        isExplosing = true;
        anim.SetTrigger("Explode");
        ExplosionManager.instance.CreateExplosion(transform.position, shockWaveForce);

        Destroy(gameObject, explosionDuration);
    }

    #region Gizmos/OnValidate

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
