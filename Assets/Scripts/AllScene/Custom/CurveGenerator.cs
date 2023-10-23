using UnityEngine;
using Collision2D;

public class CurveGenerator : MonoBehaviour
{
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
        if(isAcontrolPointSelectedLasFrame)
        {
            if (InputManager.GetKey(KeyCode.Mouse0))
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
            if(InputManager.GetKey(KeyCode.Mouse0))
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

/*
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
*/
