using System.Collections.Generic;
using UnityEngine;

public class Electrode : MonoBehaviour
{
    private LayerMask charMask;
    private bool isActive;
    private float lastTimeTriggerIsActive = -10f;
    private List<uint> charAlreadyTouch = new List<uint>();
    private Vector2[][] toricInterPoints;
    private Transform lineRenderersParent;
    private LineRenderer lineRendererPrefabs;
    private List<LineRenderer> lineRenderers;
    private bool firstTimeIsActive;

    [SerializeField] private bool enableBehaviour = true;
    [SerializeField] private Transform electrode1, electrode2;
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
        enableBehaviour = false;
        Invoke(nameof(ActiveSelf), timeOffset);
        PauseManager.instance.callBackOnPauseDisable += Enable;
        PauseManager.instance.callBackOnPauseEnable += Disable;
    }

    #region Update

    private void Update()
    {
        if(!enableBehaviour)
        {
            ClearLineRenderer();
            return;
        }

        if (isActive)
        {
            Vector2 dir = PhysicsToric.Direction(electrode1.position, electrode2.position);
            float distance = PhysicsToric.Distance(electrode1.position, electrode2.position);
            RaycastHit2D[] raycasts = PhysicsToric.RaycastAll(electrode1.position, dir, distance, charMask, out toricInterPoints);

            foreach (RaycastHit2D raycast in raycasts)
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

            if (firstTimeIsActive)
            {
                UpdateLineRenderer();
                firstTimeIsActive = false;
            }

            if (Time.time - lastTimeTriggerIsActive > activationDuration)
            {
                isActive = false;
                lastTimeTriggerIsActive = Time.time;
            }
        }
        else
        {
            ClearLineRenderer();

            if (Time.time - lastTimeTriggerIsActive > durationBetween2Activation)
            {
                isActive = true;
                firstTimeIsActive = true;
                lastTimeTriggerIsActive = Time.time;
            }
        }

        void UpdateLineRenderer()
        {
            if(PhysicsToric.GetToricIntersection(electrode1.position, electrode2.position, out Vector2 inter))
            {
                SetLineRendererCount(2);
                Vector2 inter2 = PhysicsToric.GetComplementaryPoint(inter);
                if (inter.SqrDistance(inter2) < 1)
                    print("debuuuuug");
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

        void ClearLineRenderer()
        {
            foreach (LineRenderer lr in lineRenderers)
            {
                lr.positionCount = 0;
                lr.SetPositions(new Vector3[0]);
                Destroy(lr.gameObject);
            }
            lineRenderers.Clear();
        }
    }

    #endregion

    private void ActiveSelf()
    {
        enableBehaviour = isActive = firstTimeIsActive = true;
        lastTimeTriggerIsActive = Time.time;
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

    private void OnValidate()
    {
        timeOffset = Mathf.Max(timeOffset, 0f);
        durationBetween2Activation = Mathf.Max(durationBetween2Activation, 0f);
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

    #endregion
}
