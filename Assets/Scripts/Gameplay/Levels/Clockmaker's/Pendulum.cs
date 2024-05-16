using System;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    private float accAng, vAng, ang, MgOl, oldAng;
    private LayerMask charMask;
    private new Transform transform;

    public bool enableBehaviour = true;
    [SerializeField] private float length = 3f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField, Range(-180f, 180f)] private float angleInit = 45f;
    [SerializeField] private float initAngularVelocity = 0f;
    [Tooltip("The oscillating speed."), SerializeField] private float gravityScale = 1f;

    [HideInInspector] public Action callbackOnPendulumTick;
    public float oscilatingDuration => 2f * Mathf.PI * Mathf.Sqrt(length / -Physics2D.gravity.y);

    private void Awake()
    {
        callbackOnPendulumTick = () => { };
        charMask = LayerMask.GetMask("Char");
        this.transform = base.transform;
    }

    private void Start()
    {
        accAng = 0f;
        vAng = initAngularVelocity;
        ang = angleInit * Mathf.Deg2Rad;
        oldAng = ang;
        MgOl = (Physics2D.gravity.y * gravityScale) / length;
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;

        accAng = MgOl * Mathf.Sin(ang);
        vAng += accAng * Time.deltaTime;
        ang += vAng * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, ang * Mathf.Rad2Deg);

        Vector2 pendulumPos = GetPendulumPosition();
        Collider2D[] cols = PhysicsToric.OverlapCircleAll(pendulumPos, radius, charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                EventController ec = player.GetComponent<EventController>();
                ec.OnBeenTouchByEnvironnement(gameObject);
            }
        }

        if((int)oldAng.Sign() != (int)ang.Sign())
        {
            callbackOnPendulumTick.Invoke();
        }
        oldAng = ang;
    }

    private Vector2 GetPendulumPosition() => (Vector2)transform.position + Useful.Vector2FromAngle(ang - Mathf.PI * 0.5f, length);

    #region Pause/OnValidate/Gizmos

    private void Enable()
    {
        enableBehaviour = true;
    }

    private void Disable()
    {
        enableBehaviour = false;
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= Disable;
        PauseManager.instance.callBackOnPauseDisable -= Enable;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Vector2 pos = GetPendulumPosition();
        Collision2D.Circle.GizmosDraw(pos, radius, Color.green, true);
    }

    private void OnValidate()
    {
        this.transform = base.transform;
        gravityScale = Mathf.Max(0f, gravityScale);
        transform.rotation = Quaternion.Euler(0f, 0f, angleInit);
        ang = angleInit * Mathf.Deg2Rad;
    }

#endif

    #endregion
}
