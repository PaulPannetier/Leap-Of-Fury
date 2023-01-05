using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CurveGenerator : MonoBehaviour
{
    #region Static members

    private static float cache0;
    public static Vector2 CubicBezierCurve(in Vector2 start, in Vector2 handle1, in Vector2 handle2, in Vector2 end, float t)
    {
        cache0 = t * t * t;
        return start * (-cache0 + 3f * t * t - 3f * t + 1f) + handle1 * (3f * cache0 - 6f * t * t + 3f * t) + handle2 * (-3f * cache0 + 3f * t * t) + end * cache0;
    }

    public static Vector2 CubicBezierCurveVelocity(in Vector2 start, in Vector2 handle1, in Vector2 handle2, in Vector2 end, float t)
    {
        cache0 = t * t;
        return start * (-3f * cache0 + 6f * t - 3f) + handle1 * (9f * cache0 - 12f * t + 3f) + handle2 * (-9f * cache0 + 6f * t) + end * (3f * cache0);
    }

    public static Hitbox CubicBezierCurveHitbox(in Vector2 start, in Vector2 handle1, in Vector2 handle2, in Vector2 end)
    {
        //on recherche les t €[0,1] | P'(t).x == 0 || P'(t).y == 0
        float[] t = new float[4] { -1f, -1f, -1f, -1f };
        Vector2 a = -3f * start + 9f * handle1 - 9f * handle2 + 3f * end;
        Vector2 b = 6f * start - 12f * handle1 + 6f * handle2;
        Vector2 c = -3f * start + 3f * handle1;

        cache0 = (b.x * b.x) - (4f * a.x * c.x);
        if(cache0 >= 0f)
        {
            if(cache0 <= 1e-6)
            {
                t[0] = -b.x / (2f * a.x);
                VerifyTValue(ref t, 0);
            }
            else
            {
                float sqrDelta = Mathf.Sqrt(cache0);
                t[0] = (-b.x + sqrDelta) / (2f * a.x);
                t[1] = (-b.x - sqrDelta) / (2f * a.x);
                VerifyTValue(ref t, 0);
                VerifyTValue(ref t, 1);
            }
        }

        cache0 = (b.y * b.y) - (4f * a.y * c.y);
        if (cache0 >= 0f)
        {
            if (cache0 <= 1e-6)
            {
                t[2] = -b.y / (2f * a.y);
                VerifyTValue(ref t, 2);
            }
            else
            {
                float sqrDelta = Mathf.Sqrt(cache0);
                t[2] = (-b.y + sqrDelta) / (2f * a.y);
                t[3] = (-b.y - sqrDelta) / (2f * a.y);
                VerifyTValue(ref t, 2);
                VerifyTValue(ref t, 3);
            }
        }

        void VerifyTValue(ref float[] t, int index)
        {
            if (t[index] < 0f || t[index] > 1f)
                t[index] = -1f;
        }

        List<Vector2> extremaPoints = new List<Vector2>()
        {
            start, end
        };

        for (int i = 0; i < 4; i++)
        {
            if (t[i] >= 0f)
            {
                extremaPoints.Add(CubicBezierCurve(start, handle1, handle2, end, t[i]));
            }
        }

        //on crée la boite de collision avec les points extremes
        float xMin = extremaPoints[0].x, xMax = extremaPoints[0].x, yMin = extremaPoints[0].y, yMax = extremaPoints[0].y;

        for (int i = 1; i < extremaPoints.Count; i++)
        {
            xMin = Mathf.Min(xMin, extremaPoints[i].x);
            xMax = Mathf.Max(xMax, extremaPoints[i].x);
            yMin = Mathf.Min(yMin, extremaPoints[i].y);
            yMax = Mathf.Max(yMax, extremaPoints[i].y);
        }

        return new Hitbox(new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f), new Vector2(xMax - xMin, yMax - yMin));
    }

    public static Vector2 HermiteCurve(in Vector2 start, in Vector2 v0, in Vector2 v1, in Vector2 end, float t)
    {
        cache0 = t * t * t;
        return start * (1f - 3f * t * t + 2f * cache0) + v0 * (t - 2f * t * t + cache0) + end * (3f * t * t - 2f * cache0) + v1 * (-1f * t * t + cache0);
    }

    public static Vector2 HermiteCurveVelocity(in Vector2 start, in Vector2 v0, in Vector2 v1, in Vector2 end, float t)
    {
        return CubicBezierCurveVelocity(start, start + v0 * 0.3333333333f, end - v1 * 0.3333333333f, end, t);
    }

    public static Hitbox HermiteCurveHitbox(in Vector2 start, in Vector2 v0, in Vector2 v1, in Vector2 end)
    {
        return CubicBezierCurveHitbox(start, start + v0 * 0.3333333333f, end - v1 * 0.3333333333f, end);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controlPoints"></param>
    /// <param name="handlePoints"></param>
    /// <returns>La courbe de bézier passant par les points controlPoints et la courbe sera influencé par les handlePoints</returns>
    public static Vector2[] CubicBezierSpline(in Vector2[] controlPoints, in Vector2[] handlePoints, int pointsPerCurve)
    {
        //verif
        if(handlePoints.Length != 2 * controlPoints.Length - 2)
        {
            Debug.LogError("wrong parameter for calculate bezier spline : " + handlePoints.Length + " != " + (2 * controlPoints.Length - 2));
            return null;
        }

        Vector2[] res = new Vector2[(controlPoints.Length - 1) * pointsPerCurve - controlPoints.Length + 2];
        int resIndex = 0;
        float step = 1f / (pointsPerCurve - 1);
        float t;
        Vector2 start, end, handle1, handle2;

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            start = controlPoints[i];
            end = controlPoints[i + 1];
            handle1 = handlePoints[2 * i];
            handle2 = handlePoints[2 * i + 1];
            res[resIndex] = start;
            resIndex++;
            for (int j = 1; j < pointsPerCurve - 1; j++)
            {
                t = step * j;
                res[resIndex] = CubicBezierCurve(start, handle1, handle2, end, t);
                resIndex++;
            }
        }
        res[resIndex] = controlPoints[controlPoints.Length - 1];
        return res;
    }

    public static Hitbox[] CubicBezierSplineHitboxes(in Vector2[] controlPoints, in Vector2[] handlePoints)
    {
        Hitbox[] res = new Hitbox[controlPoints.Length - 1];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = CubicBezierCurveHitbox(controlPoints[i], handlePoints[2 * i], handlePoints[2 * i + 1], controlPoints[i + 1]);
        }
        return res;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controlPoints"></param>
    /// <param name="handlePoints"></param>
    /// <returns>La courbe de Catmull-Rom passant par les points controlPoints</returns>
    public static Vector2[] CatmullRomSpline(in Vector2[] controlPoints, int pointsPerCurve)
    {
        Vector2[] newControlPoints = new Vector2[controlPoints.Length + 2];
        newControlPoints[0] = 2f * controlPoints[0] - controlPoints[1];
        newControlPoints[newControlPoints.Length - 1] = 2f * controlPoints[controlPoints.Length - 1] - controlPoints[controlPoints.Length - 2];
        for (int i = 1; i < newControlPoints.Length - 1; i++)
        {
            newControlPoints[i] = controlPoints[i - 1];
        }

        Vector2[] handles = new Vector2[controlPoints.Length];
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] = (newControlPoints[i+2] - newControlPoints[i]) * 0.5f;
        }

        Vector2[] res = new Vector2[(controlPoints.Length - 1) * pointsPerCurve - controlPoints.Length + 2];
        int resIndex = 0;
        float step = 1f / (pointsPerCurve - 1);
        float t;
        Vector2 start, end, v0, v1;

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            start = controlPoints[i];
            end = controlPoints[i + 1];
            v0 = handles[i];
            v1 = handles[i + 1];
            res[resIndex] = start;
            resIndex++;
            for (int j = 1; j < pointsPerCurve - 1; j++)
            {
                t = step * j;
                res[resIndex] = HermiteCurve(start, v0, v1, end, t);
                resIndex++;
            }
        }
        res[resIndex] = controlPoints[controlPoints.Length - 1];
        return res;
    }

    public static Hitbox[] CatmullRomSplineHitboxes(in Vector2[] controlPoints)
    {
        Vector2[] newControlPoints = new Vector2[controlPoints.Length + 2];
        newControlPoints[0] = 2f * controlPoints[0] - controlPoints[1];
        newControlPoints[newControlPoints.Length - 1] = 2f * controlPoints[controlPoints.Length - 1] - controlPoints[controlPoints.Length - 2];
        for (int i = 1; i < newControlPoints.Length - 1; i++)
        {
            newControlPoints[i] = controlPoints[i - 1];
        }

        Vector2[] handles = new Vector2[controlPoints.Length];
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] = (newControlPoints[i + 2] - newControlPoints[i]) * 0.5f;
        }

        Hitbox[] res = new Hitbox[controlPoints.Length - 1];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = HermiteCurveHitbox(controlPoints[i], handles[i], handles[i + 1], controlPoints[i+1]);
        }
        return res;
    }

    #endregion

    private EdgeCollider2D edgeCollider;
    private Vector2[] curve;

    [Header("Collider")]
    [SerializeField, HideInInspector] private bool useCollider = false;
    [SerializeField, HideInInspector] private float colliderEdgeRadius;
    [SerializeField, HideInInspector] private int colliderResolutionPerCurve = 7;

    [Header("Shape")]
    [SerializeField] private Vector2[] controlPoints;
    [SerializeField] private int pointsPerCurve = 70;

    private void Awake()
    {
        if(useCollider)
        {
            edgeCollider = GetComponent<EdgeCollider2D>();
            if(edgeCollider == null)
            {
                AddEdgeCollider();
            }
        }
    }

    private void Start()
    {
        CalculateCurve();
    }

    private void AddEdgeCollider()
    {
        edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        edgeCollider.edgeRadius = colliderEdgeRadius;
    }

    public void CalculateCurve()
    {
        curve = CatmullRomSpline(controlPoints, pointsPerCurve);
        if(useCollider)
        {
            if(edgeCollider == null)
                AddEdgeCollider();

            edgeCollider.edgeRadius = colliderEdgeRadius;

            Vector2[] colliderPoints = new Vector2[(controlPoints.Length - 1) * colliderResolutionPerCurve];
            int indexColliderPoints = 0;
            int gap = Mathf.Max(pointsPerCurve / colliderResolutionPerCurve, 1);
            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                for (int j = 0; j < colliderResolutionPerCurve; j++)
                {
                    if (indexColliderPoints >= colliderPoints.Length || i * pointsPerCurve + j * gap >= curve.Length)
                        break;
                    colliderPoints[indexColliderPoints] = curve[i * pointsPerCurve + j * gap];
                    indexColliderPoints++;
                }
            }
            colliderPoints[colliderPoints.Length - 1] = curve[curve.Length - 1];

            edgeCollider.points = colliderPoints;
        }
    }

    private bool isAcontrolPointSelectedLasFrame = false;
    private int indexSelectedPoint = -1;
    [ExecuteInEditMode()]
    private void UpdateControlePoints()
    {
        print("dqsd");
        if(isAcontrolPointSelectedLasFrame)
        {
            if (CustomInput.GetKey(KeyCode.Mouse0))
            {
                Vector2 mousePos = Useful.mainCamera.ScreenToWorldPoint(Input.mousePosition);
                controlPoints[indexSelectedPoint] = mousePos;
            }
            else
            {
                isAcontrolPointSelectedLasFrame = false;
            }
        }
        else
        {
            if(CustomInput.GetKey(KeyCode.Mouse0))
            {
                for (int i = 0; i < controlPoints.Length; i++)
                {
                    Vector2 mousePos = Useful.mainCamera.ScreenToWorldPoint(Input.mousePosition);
                    if (mousePos.SqrDistance(controlPoints[i]) < 0.3f * 0.3f)
                    {
                        isAcontrolPointSelectedLasFrame = true;
                        indexSelectedPoint = i;
                        break;
                    }
                }
            }
        }
    }

    #region Gizmos/OnValidate

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Vector2 v in controlPoints)
        {
            Circle.GizmosDraw(v, 0.3f);
        }

        CalculateCurve();

        Vector2 previous = curve[0];
        for (int i = 1; i < curve.Length; i++)
        {
            Gizmos.DrawLine(previous, curve[i]);
            previous = curve[i];
        }

        Hitbox[] hitboxes = CatmullRomSplineHitboxes(controlPoints);
        Gizmos.color = Color.red;
        foreach (Hitbox hitbox in hitboxes)
        {
            if(hitbox != null)
                Hitbox.GizmosDraw(hitbox);
        }
    }

    private void OnValidate()
    {
        if(useCollider)
        {
            edgeCollider = GetComponent<EdgeCollider2D>();
            if(edgeCollider == null)
            {
                AddEdgeCollider();
            }
        }
        colliderEdgeRadius = Mathf.Max(0f, colliderEdgeRadius);
        colliderResolutionPerCurve = Mathf.Max(1, colliderResolutionPerCurve);
        pointsPerCurve = Mathf.Max(0, pointsPerCurve); 
    }

    #endregion

}

[CustomEditor(typeof(CurveGenerator))]
public class CurveGeneratorEditor : Editor
{
    // this are serialized variables in YourClass
    SerializedProperty useCollider;
    SerializedProperty colliderEdgeRadius;
    SerializedProperty colliderResolutionPerCurve;

    private void OnEnable()
    {
        useCollider = serializedObject.FindProperty("useCollider");
        colliderEdgeRadius = serializedObject.FindProperty("colliderEdgeRadius");
        colliderResolutionPerCurve = serializedObject.FindProperty("colliderResolutionPerCurve");
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        EditorGUILayout.PropertyField(useCollider);

        if (useCollider.boolValue)
        {
            EditorGUILayout.PropertyField(colliderEdgeRadius);
            EditorGUILayout.PropertyField(colliderResolutionPerCurve);
        }

        // must be on the end.
        serializedObject.ApplyModifiedProperties();

        // add this to render base
        base.OnInspectorGUI();
    }
}
