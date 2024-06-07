using System.Collections.Generic;
using UnityEngine;

public class Electrode : MonoBehaviour
{
    private LayerMask charMask;
    private float lastTimeTriggerIsActive = -10f;
    private bool isActive;
    private List<uint> charAlreadyTouch = new List<uint>();
    private Vector2[][] toricInterPoints;
    private Transform lineRenderersParent;

    [SerializeField] private bool enableBehaviour = true;

    [Header("Position")]
    [SerializeField] private Transform electrode1;
    [SerializeField] private Transform electrode2;
    [SerializeField] private Transform ray;

    [Header("Use with trigger")]
    [SerializeField] private bool useByInterruptor = false;
    [SerializeField] private Interruptor interruptor;

    [Header("Duration Setings")]
    [SerializeField] private float timeOffset = 0f;
    [SerializeField] private float activationDuration;
    [SerializeField] private float durationBetween2Activation;

    private void Awake()
    {
        charMask = LayerMask.GetMask("Char");
    }

    private void Start()
    {
        if(useByInterruptor)
        {
            if (interruptor.isActivated)
                EnableElectrode();
            else
                DisableElectrode();
        }
        else
        {
            if (timeOffset > 0f)
            {
                DisableElectrode();
                Invoke(nameof(EnableElectrode), timeOffset);
            }
            else
            {
                EnableElectrode();
            }
        }
    }

    #region Update

    private void Update()
    {
        if(PauseManager.instance.isPauseEnable)
        {
            lastTimeTriggerIsActive += Time.deltaTime;
            return;
        }

        if (!enableBehaviour)
            return;

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
                    DisableElectrode();
                }
            }
            else
            {
                if (Time.time - lastTimeTriggerIsActive > activationDuration)
                {
                    DisableElectrode();
                }
            }
        }
        else
        {
            if(useByInterruptor)
            {
                if (interruptor.isActivated)
                {
                    EnableElectrode();
                }
            }
            else
            {
                if (Time.time - lastTimeTriggerIsActive > durationBetween2Activation)
                {
                    EnableElectrode();
                }
            }
        }
    }

    private void EnableElectrode()
    {
        isActive = true;
        ray.gameObject.SetActive(true);
        lastTimeTriggerIsActive = Time.time;
    }

    private void DisableElectrode()
    {
        isActive = false;
        ray.gameObject.SetActive(false);
    }

    #endregion

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        timeOffset = Mathf.Max(timeOffset, 0f);
        durationBetween2Activation = Mathf.Max(durationBetween2Activation, 0f);
    }

#endif

#endregion
}
