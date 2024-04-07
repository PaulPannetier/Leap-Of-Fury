using System.Collections.Generic;
using UnityEngine;

public class Electrode : MonoBehaviour
{
    public enum Direction
    {
        horizontal,
        vertical,
        freeLayout
    }

    private LayerMask charMask;
    private float lastTimeTriggerIsActive = -10f;
    private bool isActive;
    private List<uint> charAlreadyTouch = new List<uint>();
    private Vector2[][] toricInterPoints;
    private Transform lineRenderersParent;
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lineRenderers;

    [SerializeField] private bool enableBehaviour = true;

    [Header("Position")]
    [SerializeField] private Transform electrode1;
    [SerializeField] private Transform electrode2;

#if UNITY_EDITOR

    [SerializeField] private Direction direction = Direction.vertical;
    [SerializeField] private float electrodesDistance = 3f;

#endif

    [Header("Use with trigger")]
    [SerializeField] private bool useByInterruptor = false;
    [SerializeField] private Interruptor interruptor;

    [Header("Duration Setings")]
    [SerializeField] private float timeOffset = 0f;
    [SerializeField] private float activationDuration;
    [SerializeField] private float durationBetween2Activation;

    private void Awake()
    {
        if (transform.childCount < 2)
            Debug.LogWarning("The child width all the line renderer need to be the third child of this gameobject");
        lineRenderersParent = transform.GetChild(2);
        if (lineRenderersParent.childCount < 1)
            Debug.LogWarning("The child width all the line renderer need to have a child with a lineRenderer preconfigured");
        lineRendererPrefabs = lineRenderersParent.GetChild(0).GetComponent<LineRenderer>();
        if(lineRendererPrefabs == null)
            Debug.LogWarning("The child width all the line renderer need to have a child with a lineRenderer preconfigured");

        lineRenderers = new List<LineRenderer>();
        charMask = LayerMask.GetMask("Char");
    }

    private void Start()
    {
        if(!useByInterruptor)
        {
            Invoke(nameof(ActiveSelf), timeOffset);
        }

        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    #region Update

    private void Update()
    {
        if (!enableBehaviour)
        {
            ClearLineRenderer();
            return;
        }

        if (isActive)
        {
            Vector2 dir = PhysicsToric.Direction(electrode1.position, electrode2.position);
            float distance = PhysicsToric.Distance(electrode1.position, electrode2.position);
            ToricRaycastHit2D[] raycasts = PhysicsToric.RaycastAll(electrode1.position, dir, distance, charMask, out toricInterPoints);

            foreach (ToricRaycastHit2D raycast in raycasts)
            {
                if(raycast.collider != null)
                {
                    GameObject player = raycast.collider.gameObject;
                    if(player.CompareTag("Char"))
                    {
                        player = player.GetComponent<ToricObject>().original;
                        uint id = player.GetComponent<PlayerCommon>().id;
                        if(!charAlreadyTouch.Contains(id))
                        {
                            charAlreadyTouch.Add(id);
                            player.GetComponent<EventController>().OnBeenKillByEnvironnement(gameObject);
                        }
                    }
                }
            }

            if(useByInterruptor)
            {
                if(!interruptor.isActivated)
                {
                    isActive = false;
                    ClearLineRenderer();
                }
            }
            else
            {
                if (Time.time - lastTimeTriggerIsActive > activationDuration)
                {
                    isActive = false;
                    lastTimeTriggerIsActive = Time.time;
                }
            }
        }
        else
        {
            ClearLineRenderer();

            if(!useByInterruptor)
            {
                if (Time.time - lastTimeTriggerIsActive > durationBetween2Activation)
                {
                    isActive = true;
                    lastTimeTriggerIsActive = Time.time;
                    UpdateLineRenderer();
                }
            }
            else
            {
                if(interruptor.isActivated)
                {
                    isActive = true;
                    lastTimeTriggerIsActive = Time.time;
                    UpdateLineRenderer();
                }
            }
        }
    }

    private void UpdateLineRenderer()
    {
        if (PhysicsToric.GetToricIntersection(electrode1.position, electrode2.position, out Vector2 inter))
        {
            SetLineRendererCount(2);
            Vector2 inter2 = PhysicsToric.GetComplementaryPoint(inter);
            (inter, inter2) = PhysicsToric.Distance(electrode1.position, inter) <= PhysicsToric.Distance(electrode1.position, inter2) ?
                (inter, inter2) : (inter2, inter);
            lineRenderers[0].positionCount = 2;
            lineRenderers[1].positionCount = 2;
            lineRenderers[0].SetPositions(new Vector3[2] { electrode1.position, inter });
            lineRenderers[1].SetPositions(new Vector3[2] { inter2, electrode2.position });
        }
        else
        {
            SetLineRendererCount(1);
            lineRenderers[0].positionCount = 2;
            lineRenderers[0].SetPositions(new Vector3[2] { electrode1.position, electrode2.position });
        }

        void SetLineRendererCount(int count)
        {
            while (lineRenderers.Count < count)
            {
                lineRenderers.Add(Instantiate(lineRendererPrefabs, lineRenderersParent));
            }
            while (lineRenderers.Count > count)
            {
                LineRenderer lrToRm = lineRenderers.Last();
                lrToRm.positionCount = 0;
                lrToRm.SetPositions(new Vector3[0]);
                Destroy(lrToRm.gameObject);
                lineRenderers.RemoveLast();
            }
        }
    }

    private void ClearLineRenderer()
    {
        foreach (LineRenderer lr in lineRenderers)
        {
            lr.positionCount = 0;
            lr.SetPositions(new Vector3[0]);
            Destroy(lr.gameObject);
        }
        lineRenderers.Clear();
    }

    #endregion

    private void ActiveSelf()
    {
        lastTimeTriggerIsActive = Time.time;
        isActive = true;
        enableBehaviour = true;
        UpdateLineRenderer();
    }

    #region OnDrawizmos/OnValidate

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

#if UNITY_EDITOR

    private void OnValidate()
    {
        timeOffset = Mathf.Max(timeOffset, 0f);
        durationBetween2Activation = Mathf.Max(durationBetween2Activation, 0f);
        electrodesDistance = Mathf.Max(0f, electrodesDistance);

        if(electrode1 != null && electrode2 != null)
        {
            if (direction == Direction.vertical)
            {
                electrode1.transform.position = transform.position + Vector3.up * electrodesDistance * 0.5f;
                electrode2.transform.position = transform.position + Vector3.down * electrodesDistance * 0.5f;
            }
            else if (direction == Direction.horizontal)
            {
                electrode1.transform.position = transform.position + Vector3.right * electrodesDistance * 0.5f;
                electrode2.transform.position = transform.position + Vector3.left * electrodesDistance * 0.5f;
            }
            else
            {
                electrodesDistance = electrode1.transform.position.Distance(electrode2.transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        /*
        //Test PhysicsToric.RaycastAll 
        if(Application.isPlaying)
        {
            Vector2 mousePos = Useful.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            float dist = mousePos.Distance(testRaycastAllBeg);
            Vector2 dir = mousePos - testRaycastAllBeg;
            RaycastHit2D[] raycast = PhysicsToric.RaycastAll(testRaycastAllBeg, dir, dist, LayerMask.GetMask("Floor", "WallProjectile"), out Vector2[][] points);

            Gizmos.color = raycast.Length > 0 ? Color.red : Color.green;
            Circle.GizmosDraw(mousePos, 0.2f);

            for (int i = 0; i < raycast.Length; i++)
            {
                Gizmos.color = Color.blue;
                if (points[i].Length > 0)
                {
                    Vector2 beg = testRaycastAllBeg;
                    for (int j = 0; j < points[i].Length; j += 2)
                    {
                        Gizmos.DrawLine(beg, points[i][j]);
                        Circle.GizmosDraw(points[i][j], 0.25f);
                        beg = PhysicsToric.GetComplementaryPoint(points[i][j]);
                    }
                    Gizmos.DrawLine(points[i].Last(), raycast[i].point);
                }
                else
                {
                    Gizmos.DrawLine(testRaycastAllBeg, raycast[i].point);
                }

                Gizmos.color = Color.red;
                Circle.GizmosDraw(raycast[i].point, 0.4f);
            }
        }
        Gizmos.color = Color.green;
        Circle.GizmosDraw(testRaycastAllBeg, 0.5f);
        */
    }

#endif

#endregion
}
