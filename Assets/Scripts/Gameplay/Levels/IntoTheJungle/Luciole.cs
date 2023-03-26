using UnityEngine;

public class Luciole : MonoBehaviour
{
    private Vector2 targetedDir;

    public bool enableBehaviour = true;

    [Header("Detection")]
    [SerializeField, Range(0f, 360f)] float visionAngle = 45f;
    [SerializeField] float detectionRange = 3f;

    [Header("Stat")]
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float speed = 0.5f;

    private void Start()
    {
        LucioleManager.instance.lstLucioles.Add(this);
        targetedDir = transform.right;
    }

    private void Update()
    {
        if (!enableBehaviour)
            return;
        Vector2? otherLuciolePos = GetClosestOtherLuciolePositionInDetectionRange();
        if(otherLuciolePos != null)
        {
            targetedDir = ((Vector2)transform.position - (Vector2)otherLuciolePos).normalized;
        }
        float targetedAngle = Useful.AngleHori(Vector2.zero, targetedDir) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.MoveTowardsAngle(transform.rotation.eulerAngles.z, targetedAngle, rotationSpeed * Time.deltaTime));

        transform.position += (speed * Time.deltaTime) * transform.right;
    }

    private Vector2? GetClosestOtherLuciolePositionInDetectionRange()
    {
        int minLucioleIndex = -1;
        float minSqrDist = float.MaxValue;
        for (int i = 0; i < LucioleManager.instance.lstLucioles.Count; i++)
        {
            Luciole luciole = LucioleManager.instance.lstLucioles[i];
            if (luciole != this && IsInDetectionRange(luciole.transform.position))
            {
                float sqrDist = transform.position.SqrDistance(luciole.transform.position);
                if(sqrDist < minSqrDist)
                {
                    minSqrDist = sqrDist;
                    minLucioleIndex = i;
                }
            }
        }
        if(minLucioleIndex == -1)
            return null;
        return LucioleManager.instance.lstLucioles[minLucioleIndex].transform.position;
    }

    private bool IsInDetectionRange(in Vector2 pos)
    {
        float angle = Mathf.Abs(Useful.AngleHori(Vector2.zero, transform.right) - Useful.AngleHori(transform.position, pos));
        if(angle <= visionAngle * 0.5f)
            return pos.SqrDistance(transform.position) <= detectionRange * detectionRange;
        return false;
    }

    private void OnValidate()
    {
        detectionRange = Mathf.Max(0f, detectionRange);
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
        speed = Mathf.Max(0f, speed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        float angleInit = Useful.AngleHori(transform.position, transform.position + transform.right) - visionAngle * Mathf.Deg2Rad * 0.5f;
        int nbPoint = Mathf.Max(10, (int)(visionAngle));
        float step = (visionAngle / nbPoint) * Mathf.Deg2Rad;
        Vector2 oldPoint = transform.position;
        Vector2 newPoint;

        for (int i = 0; i < nbPoint; i++)
        {
            float ang = angleInit + i * step;
            newPoint = new Vector2(transform.position.x + detectionRange * Mathf.Cos(ang), transform.position.y + detectionRange * Mathf.Sin(ang));
            Gizmos.DrawLine(oldPoint, newPoint);
            oldPoint = newPoint;
        }
        Gizmos.DrawLine(oldPoint, (Vector2)transform.position);
    }
}
