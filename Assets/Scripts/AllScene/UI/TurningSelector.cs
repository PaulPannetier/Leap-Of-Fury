using System.Collections.Generic;
using UnityEngine;

public class TurningSelector : MonoBehaviour
{
    private float turningAngle;
    private int selectedIndex = 0;
    private GameObject[] itemsGO;
    private float[] itemsAngles;
    private float lastTimeMove = -10f;
    private float turningSign = 1f;
    private float angle = 0f;

    [SerializeField] private bool RecalculatePositionAndScale = false;
    public bool enableBehaviour = true;
    [SerializeField] private float radius;
    [SerializeField] private float minTimeBetweenMove = 0.2f;
    [SerializeField] private float itemsScaleMultiplier = 1f;
    [SerializeField] private AnimationCurve itemScaleByDistance;
    [field:SerializeField] public Vector3 center { get; private set; } = Vector3.zero;
    [Tooltip("Angular speed in degrees/sec")][SerializeField] private float angularSpeed = 360f;
    [SerializeField] private bool isHorizontal = true;
    [SerializeField] private bool isInvers = false;

    public GameObject selectedItem => itemsGO[selectedIndex];

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
        for (int i = 0; i < itemsGO.Length; i++)
        {
            float angle = CalculateAngle(i);
            Vector3 pos = CalculateCanvasPosition(i);
            GameObject tmpGO = transform.GetChild(i).gameObject;
            tmpGO.transform.position = pos;
            float scale = itemScaleByDistance.Evaluate(CalculateDepthDistanceProportion(pos));
            tmpGO.transform.localScale = new Vector3(scale, scale, 1f);
            itemsGO[i] = tmpGO;
            itemsAngles[i] = angle;
        }
        SortChildren();
    }

    private float CalculateAngle(int index) => Useful.WrapAngle(angle + (isHorizontal ? 1.5f * Mathf.PI + index * turningAngle : Mathf.PI + index * turningAngle));

    private Vector3 CalculateCanvasPosition(int index) => CalculateCanvasPosition(CalculateAngle(index));

    private Vector3 CalculateCanvasPosition(float angle)
    {
        return isHorizontal ? center + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle)) :
            center + new Vector3(0f, radius * Mathf.Sin(angle), radius * Mathf.Cos(angle));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pos"></param>
    /// <returns>a float between 0 and 1 proportional to the depth of the position</returns>
    private float CalculateDepthDistanceProportion(in Vector3 pos) => (((center.z - pos.z) / radius) + 1f) * 0.5f;

    private void Update()
    {
        if (!enableBehaviour)
            return;

        for (int i = 0; i < itemsGO.Length; i++)
        {
            if(Useful.AngleDist(CalculateAngle(i), itemsAngles[i]) >= Time.deltaTime * angularSpeed * Mathf.Deg2Rad)
            {
                GameObject tmpCanvasGO = itemsGO[i];
                itemsAngles[i] = Useful.WrapAngle(itemsAngles[i] + turningSign * Time.deltaTime * angularSpeed * Mathf.Deg2Rad);
                tmpCanvasGO.transform.position = CalculateCanvasPosition(itemsAngles[i]);
                GameObject tmpItemsGO = itemsGO[i];
                float scale = itemScaleByDistance.Evaluate(CalculateDepthDistanceProportion(tmpCanvasGO.transform.position)) * itemsScaleMultiplier;
                tmpItemsGO.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        SortChildren();
    }

    public void SelectedNextItem()
    {
        if (Time.time - lastTimeMove < minTimeBetweenMove)
            return;
        selectedIndex = (selectedIndex + 1) % itemsGO.Length;
        turningSign = isInvers ? -1f : 1f;
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
        turningSign = isInvers ? 1f : -1f;
        angle = Useful.WrapAngle(angle + turningSign * turningAngle);
        lastTimeMove = Time.time;
    }

    private void SortChildren()
    {
        List<Transform> children = new List<Transform> ();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        children.Sort(new TransformComparer());
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }

    private class TransformComparer : IComparer<Transform>
    {
        public int Compare(Transform x, Transform y)
        {
            return Mathf.Abs(x.position.z - y.position.z) < 1e-5 ? 0 : (x.position.z - y.position.z > 0f ?  -1 : 1);
        }
    }

    #region OnValidate/Gizmos

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, 0.5f);
        Circle.GizmosDraw(center, 25f);
    }

    private void OnValidate()
    {
        radius = Mathf.Max(0f, radius);
        angularSpeed = Mathf.Max(0f, angularSpeed);

        if(RecalculatePositionAndScale)
        {
            RecalculatePositionAndScale = false;
            Initialized();
        }
    }

    #endregion
}