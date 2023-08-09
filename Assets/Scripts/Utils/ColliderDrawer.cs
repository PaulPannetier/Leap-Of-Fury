using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ColliderDrawer : MonoBehaviour
{
    #if UNITY_EDITOR

    private bool isDrawing;
    private Vector2 point0, point1;
    private Hitbox currentHitbox;
    private List<Hitbox> recToAdd = new List<Hitbox>();

    public bool enableBehaviour = true;
    public bool AddCreatedBoxCollider2D;
    public bool removeAllHitbox;
    public bool clearSavedCollider;
    public bool drawGrid = true;
    [SerializeField] private GameObject goToAddBoxCollider2D;
    [SerializeField] private Vector2 caseSize = Vector2.one, gridSize = new Vector2(32f, 18f);
    [SerializeField] private Vector2 gridCenter = Vector2.zero;
    [SerializeField] private KeyCode inputToStartDrawing = KeyCode.A;
    [SerializeField] private KeyCode inputToUndo = KeyCode.Z;
    [SerializeField] private KeyCode inputToSaveCollider = KeyCode.S;

    #endif

    private void Awake()
    {
        if(!Application.isEditor)
        {
            Destroy(this);
            return;
        }
    }

    #if UNITY_EDITOR

    [ExecuteAlways]
    private void Update()
    {
        if (removeAllHitbox)
        {
            RemoveBoxColliders();
            removeAllHitbox = false;
        }

        if (!enableBehaviour)
            return;

        if(recToAdd == null)
            recToAdd = new List<Hitbox>();

        if(isDrawing)
        {
            point1 = GetMousePos();
            if(InputManager.GetKeyUp(inputToStartDrawing))
            {
                print("Stop drawing");
                AddRectangle();
                isDrawing = false;
            }
            currentHitbox = GetRecFromTwoPoints(point0, point1);
        }
        else
        {
            if(InputManager.GetKeyDown(inputToStartDrawing))
            {
                print("Start drawing");
                isDrawing = true;
                point0 = GetMousePos();
                point1 = point0;
                currentHitbox = GetRecFromTwoPoints(point0, point1);
            }
        }

        if (InputManager.GetKeyDown(inputToUndo))
        {
            if(recToAdd.Count > 0)
            {
                recToAdd.RemoveLast();
            }
        }

        if (InputManager.GetKeyDown(inputToSaveCollider))
        {
            print("Collider's saved!");
            SaveCollider();
        }

        if (removeAllHitbox)
        {
            RemoveBoxColliders();
            removeAllHitbox = false;
        }
    }
    
    private void SaveCollider()
    {
        Vector2[] centers = new Vector2[recToAdd.Count], sizes = new Vector2[recToAdd.Count];
        for (int i = 0; i < recToAdd.Count; i++)
        {
            centers[i] = recToAdd[i].center;
            sizes[i] = recToAdd[i].size;
        }
        ColliderDrawingData data = new ColliderDrawingData(centers, sizes);
        Save.WriteJSONData(data, @"/Save/tmp/ColliderDrawer" + SettingsManager.saveFileExtension);
        recToAdd.Clear();
        isDrawing = false;
    }

    private ColliderDrawingData LoadCollider()
    {
        if(Save.ReadJSONData<ColliderDrawingData>(@"/Save/tmp/ColliderDrawer" + SettingsManager.saveFileExtension, out ColliderDrawingData data))
        {
            return data;
        }
        return default(ColliderDrawingData);
    }

    private Vector2 GetCaseCenterFromPoint(Vector2 p)
    {
        p -= gridCenter;
        int l = (p.y / caseSize.y).Floor();
        int c = (p.x / caseSize.x).Floor();
        return new Vector2((c + 0.5f) * caseSize.x, (l + 0.5f) * caseSize.y) + gridCenter;
    }

    private Hitbox GetRecFromTwoPoints(Vector2 p0, Vector2 p1)
    {
        p0 = GetCaseCenterFromPoint(p0);
        p1 = GetCaseCenterFromPoint(p1);
        return new Hitbox((p0 + p1) * 0.5f, new Vector2(Mathf.Abs(p0.x - p1.x) + caseSize.x, Mathf.Abs(p0.y - p1.y) + caseSize.y));
    }

    private void AddRectangle()
    {
        recToAdd.Add(currentHitbox);
    }

    private Vector2 GetMousePos() => Useful.mainCamera.ScreenToWorldPoint(InputManager.mousePosition);

    #region Gizmos/OnValidate

    private void RemoveBoxColliders()
    {
        BoxCollider2D[] hitboxs = goToAddBoxCollider2D.GetComponents<BoxCollider2D>();
        foreach (BoxCollider2D hitbox in hitboxs)
        {
            DestroyImmediate(hitbox);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(drawGrid)
        {
            Gizmos.color = Color.white;
            Vector2 offset = gridCenter + gridSize * caseSize * -0.5f + caseSize * 0.5f;
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    Hitbox.GizmosDraw(new Vector2(x * caseSize.x, y * caseSize.y) + offset, caseSize);
                }
            }
        }

        Gizmos.color = Color.red;
        Circle.GizmosDraw(gridCenter, 0.1f);

        Gizmos.color = Color.green;
        foreach (Hitbox rec in recToAdd)
        {
            Hitbox.GizmosDraw(rec);
        }

        if(isDrawing)
        {
            Hitbox.GizmosDraw(currentHitbox);
        }
    }

    private void OnValidate()
    {
        caseSize = new Vector2(Mathf.Max(0f, caseSize.x), Mathf.Max(0f, caseSize.y));
        gridSize = new Vector2(Mathf.Max(0f, gridSize.x), Mathf.Max(0f, gridSize.y));

        if(AddCreatedBoxCollider2D)
        {
            recToAdd = new List<Hitbox>();
            ColliderDrawingData data = LoadCollider();
            for (int i = 0; i < data.center.Length; i++)
            {
                BoxCollider2D boxCollider = goToAddBoxCollider2D.AddComponent<BoxCollider2D>();
                boxCollider.offset = data.center[i] - (Vector2)transform.position;
                boxCollider.size = data.size[i];
            }
            AddCreatedBoxCollider2D = false;
        }

        if(clearSavedCollider)
        {
            recToAdd.Clear();
            SaveCollider();
            clearSavedCollider = false;
        }
    }

    #endregion

    [System.Serializable]
    private struct ColliderDrawingData
    {
        public Vector2[] center, size;

        public ColliderDrawingData(Vector2[] center, Vector2[] size)
        {
            this.center = center;
            this.size = size;
        }
    }

    #endif
}
