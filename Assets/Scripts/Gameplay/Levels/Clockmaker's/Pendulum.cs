using UnityEngine;

public class Pendulum : MonoBehaviour
{
    private float accAng, vAng, ang, MgOl;
    private GameObject line, pendulumCol;

    [SerializeField] private float length = 3f;
    [SerializeField] private float radius = 1.5f;
    [SerializeField, Range(0f, 360f)] private float angleInit = 45f;
    [SerializeField] private float initAngularVelocity = 0f;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private LayerMask charMask;

    private void Awake()
    {
        line = transform.GetChild(0).gameObject;
        pendulumCol = transform.GetChild(1).gameObject;
    }

    private void Start()
    {
        accAng = 0f;
        vAng = initAngularVelocity;
        ang = angleInit * Mathf.Deg2Rad;
        MgOl = (Physics2D.gravity.y * gravityScale) / length;
    }

    private void Update()
    {
        accAng = MgOl * Mathf.Sin(ang);
        vAng += accAng * Time.deltaTime;
        ang += vAng * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, ang * Mathf.Rad2Deg);

        Collider2D[] cols = PhysicsToric.OverlapCircleAll(pendulumCol.transform.position, radius, charMask);
        foreach (Collider2D col in cols)
        {
            if(col.CompareTag("Char"))
            {
                GameObject player = col.GetComponent<ToricObject>().original;
                EventController ec = player.GetComponent<EventController>();
                ec.OnBeenTouchByEnvironnement(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pendulumCol == null)
            pendulumCol = transform.GetChild(1).gameObject;

        Gizmos.color = Color.green;
        Circle.GizmosDraw(pendulumCol.transform.position, radius);
    }

    private void OnValidate()
    {
        gravityScale = Mathf.Max(0f, gravityScale);
        if (line == null)
            line = transform.GetChild(0).gameObject;
        if (pendulumCol == null)
            pendulumCol = transform.GetChild(1).gameObject;

        line.transform.localPosition = new Vector3(0f, -length * 0.5f, 0f);
        line.transform.localScale = new Vector3(line.transform.localScale.x, -length, 0f);
        pendulumCol.transform.localPosition = new Vector3(0f, -length, 0f);
        pendulumCol.transform.localScale = new Vector3(2f * radius, 2f * radius, 1f);
        transform.localRotation = Quaternion.Euler(0f, 0f, angleInit);
    }
}
