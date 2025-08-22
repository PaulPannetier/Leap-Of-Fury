using System.Collections.Generic;
using UnityEngine;
using Collision2D;

public class TurningSelector : MonoBehaviour
{
    private new Transform transform;
    private float turningAngle;
    private GameObject[] itemsGO;
    private float[] itemsAngles;
    private float[] itemsDepth;
    private float lastTimeMove = -10f;
    private float turningSign = 1f;
    private float angle = 0f;
    private int selectedIndex = 0;

#if UNITY_EDITOR
    [SerializeField] private bool RecalculatePositionAndScale = false;
#endif

    public bool enableBehaviour = true;
    [SerializeField] private float radius;
    [SerializeField] private float minTimeBetweenMove = 0.2f;
    [SerializeField] private float itemsScaleMultiplier = 1f;
    [SerializeField] private AnimationCurve itemScaleByDistance;
    [SerializeField] public Vector2 offset = Vector2.zero;
    [Tooltip("Angular speed in degrees/sec")][SerializeField] private float angularSpeed = 360f;
    [SerializeField] private bool isHorizontal = true;
    [SerializeField] private bool isInvers = false;

    public Vector2 center => (Vector2)transform.position + offset;
    public GameObject selectedItem => itemsGO[selectedIndex];

    private void Awake()
    {
        this.transform = base.transform;
    }

    private void Start()
    {
        Initialized();
    }

    private void Initialized()
    {
        selectedIndex = 0;
        angle = 0f;
        turningAngle = 2f * Mathf.PI / transform.childCount;
        itemsGO = new GameObject[transform.childCount];
        itemsAngles = new float[transform.childCount];
        itemsDepth = new float[transform.childCount];

        for (int i = 0; i < itemsGO.Length; i++)
        {
            float angle = CalculateAngle(i);
            (Vector2 pos, float depth) = CalculateCanvasPositionAndDepth(angle);
            GameObject tmpGO = transform.GetChild(transform.childCount - 1 - i).gameObject;
            tmpGO.transform.position = pos;
            itemsDepth[i] = depth;
            float scale = CalculateScale(depth);
            tmpGO.transform.localScale = new Vector3(scale, scale, 1f);
            itemsGO[i] = tmpGO;
            itemsAngles[i] = angle;
        }

        SortChildren();
    }

    private float CalculateAngle(int index) => Useful.WrapAngle(angle + index * turningAngle);

    //depth € [-1, 1]
    private (Vector2, float) CalculateCanvasPositionAndDepth(float angle)
    {
        Vector2 c = center;
        Vector2 position = isHorizontal ? new Vector2(c.x + radius * Mathf.Sin(angle), c.y) : new Vector2(c.x, c.y + radius * Mathf.Sin(angle));
        return (position, Mathf.Cos(angle));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>a float between 0 and 1 proportional to the depth of the position</returns>
    private float CalculateScale(float depth) => itemsScaleMultiplier * itemScaleByDistance.Evaluate((depth + 1f) * 0.5f);

    private void Update()
    {
        if (!enableBehaviour)
            return;

        for (int i = 0; i < itemsGO.Length; i++)
        {
            GameObject tmpCanvasGO = itemsGO[i];
            if (Useful.AngleDist(CalculateAngle(i), itemsAngles[i]) >= Time.deltaTime * angularSpeed * Mathf.Deg2Rad)
            {
                itemsAngles[i] = Useful.WrapAngle(itemsAngles[i] + turningSign * Time.deltaTime * angularSpeed * Mathf.Deg2Rad);
            }
            else
            {
                itemsAngles[i] = CalculateAngle(i);
            }

            (Vector2 pos, float depth) = CalculateCanvasPositionAndDepth(itemsAngles[i]);
            itemsDepth[i] = depth;
            tmpCanvasGO.transform.position = pos;
            float scale = CalculateScale(depth);
            tmpCanvasGO.transform.localScale = new Vector3(scale, scale, 1f);
        }

        SortChildren();
    }

    public void SelectedNextItem()
    {
        if (Time.time - lastTimeMove < minTimeBetweenMove)
            return;
        selectedIndex = (selectedIndex + 1) % itemsGO.Length;
        turningSign = isInvers ? 1f : -1f;
        turningSign = (isInvers ? -1f : 1f) * (isHorizontal ? -1f : 1f);
        angle = Useful.WrapAngle(angle + turningSign * turningAngle);
        lastTimeMove = Time.time;
    }

    public void SelectPreviousItem()
    {
        if (Time.time - lastTimeMove < minTimeBetweenMove)
            return;
        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = itemsGO.Length - 1;
        turningSign = (isInvers ? -1f : 1f) * (isHorizontal ? 1f : -1f);
        angle = Useful.WrapAngle(angle + turningSign * turningAngle);
        lastTimeMove = Time.time;
    }

    private void SortChildren()
    {
        List<SortingChildrenClass> sortingChildren = new List<SortingChildrenClass>();
        for (int i = 0; i < itemsGO.Length; i++)
        {
            sortingChildren.Add(new SortingChildrenClass(itemsGO[i].transform, itemsDepth[i]));
        }

        sortingChildren.Sort(SortingChildrenClassComparer.instance);
        for (int i = 0; i < sortingChildren.Count; i++)
        {
            sortingChildren[i].transform.SetSiblingIndex(i);
        }
    }

    private class SortingChildrenClass
    {
        public Transform transform;
        public float depth;

        public SortingChildrenClass(Transform transform, float depth)
        {
            this.transform = transform;
            this.depth = depth;
        }
    }

    private class SortingChildrenClassComparer : IComparer<SortingChildrenClass>
    {
        public static SortingChildrenClassComparer instance = new SortingChildrenClassComparer();

        public int Compare(SortingChildrenClass x, SortingChildrenClass y)
        {
            return Mathf.Abs(x.depth - y.depth) < 1e-6 ? 0 : (x.depth - y.depth > 0f ?  1 : -1);
        }
    }

    #region OnValidate/Gizmos

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, 0.5f);
        Circle.GizmosDraw(center, 25f);
    }

    private void OnValidate()
    {
        this.transform = base.transform;
        radius = Mathf.Max(0f, radius);
        angularSpeed = Mathf.Max(0f, angularSpeed);
        minTimeBetweenMove = Mathf.Max(minTimeBetweenMove, 0f);

        if(RecalculatePositionAndScale)
        {
            RecalculatePositionAndScale = false;
            Initialized();
        }
    }

#endif

#endregion
}