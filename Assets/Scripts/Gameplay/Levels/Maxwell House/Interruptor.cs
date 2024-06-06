using System;
using System.Collections;
using System.Collections.Generic;
using Collision2D;
using Collider2D = UnityEngine.Collider2D;
using UnityEngine;

public class Interruptor : MonoBehaviour
{
    private LayerMask charMask;
    private new Transform transform;
    private float lastTimeActivated = -10f;
    private bool isCharActivating, isCharDesactivating;
    private bool isCharDying;
    private GameObject charWhoActivate;
    private List<GameObject> charInFrontLastFrame;

    public bool enableBehaviour = true;
#if UNITY_EDITOR
    public bool drawGizmos = true;
#endif
    [SerializeField] private Vector2 hitboxOffset, hitboxSize;
    [SerializeField] private bool startActivated = false;
    [SerializeField] private bool allowDesactivation = true;
    [SerializeField] private float minDurationBefore2Activation;
    [SerializeField] private float durationItTakesToActivate = 1f;
    [SerializeField] private float durationItTakesToDesactivate = 1f;
    [SerializeField] private float activationDuration = -1f;//unlimited if < 0f
    [SerializeField, Tooltip("Be triggered when a player pass througt the button")] private bool dontUseInputSystem = false;

    [HideInInspector] public bool isActivated { get; private set; }
    public Action<PressedInfo> onActivate, onDesactivate;

    private void Awake()
    {
        onActivate = new Action<PressedInfo>((PressedInfo arg) => { });
        onDesactivate = new Action<PressedInfo>((PressedInfo arg) => { });
        charMask = LayerMask.GetMask("Char");
        this.transform = base.transform;
    }

    private void Start()
    {
        EventManager.instance.callbackOnPlayerDeath += OnCharDied;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement += OnCharDied;
        charInFrontLastFrame = new List<GameObject>(4);
        OnActivate(startActivated, new PressedInfo(null));
    }

    private void Update()
    {
        if (!enableBehaviour || PauseManager.instance.isPauseEnable)
        {
            lastTimeActivated += Time.deltaTime;
            return;
        }

        if (isCharActivating || isCharDesactivating)
            return;

        GetComponentInChildren<SpriteRenderer>().color = isActivated ? Color.red : Color.green;

        if (isActivated)
        {
            if((Time.time - lastTimeActivated > minDurationBefore2Activation) || allowDesactivation)
            {
                if(TryGetCharacterInteract(out GameObject charWhoPressed))
                {
                    Desactivate(charWhoPressed);
                }
            }

            if(activationDuration > 0f && Time.time - lastTimeActivated > activationDuration)
            {
                Desactivate(null);
            }
        }
        else
        {
            if (Time.time - lastTimeActivated > minDurationBefore2Activation && TryGetCharacterInteract(out GameObject charWhoPressed))
            {
                Activate(charWhoPressed);
            }
        }

        void Activate(GameObject charWhoPressed)
        {
            if(dontUseInputSystem)
            {
                OnActivate(true, new PressedInfo(charWhoPressed));
                return;
            }

            StartCoroutine(ActivateCorout(true, new PressedInfo(charWhoPressed)));
        }

        void Desactivate(GameObject charWhoPressed)
        {
            if(charWhoPressed == null)
            {
                OnActivate(false, new PressedInfo(null));
                return;
            }
            if(dontUseInputSystem)
            {
                OnActivate(false, new PressedInfo(charWhoPressed));
                return;
            }

            StartCoroutine(ActivateCorout(false, new PressedInfo(charWhoPressed)));
        }
    }

    private IEnumerator ActivateCorout(bool activate, PressedInfo pressedInfo)
    {
        isCharDying = false;
        isCharActivating = activate;
        isCharDesactivating = !activate;
        charWhoActivate = pressedInfo.charWhoPressed;
        float duration = activate ? durationItTakesToActivate : durationItTakesToDesactivate;
        float time = Time.time;
        bool cancel = false;
        CharacterController charMvt = pressedInfo.charWhoPressed.GetComponent<CharacterController>();
        charMvt.Freeze();
        while(Time.time - time < duration)
        {
            if(isCharDying)
            {
                cancel = true;
                break;
            }

            while(PauseManager.instance.isPauseEnable)
            {
                yield return null;
                time += Time.deltaTime;
            }

            yield return null;
        }

        if(!cancel)
        {
            charMvt.UnFreeze();
            OnActivate(activate, pressedInfo);
        }
        isCharActivating = isCharDesactivating = false;
    }

    private void OnActivate(bool activate, PressedInfo pressedInfo)
    {
        isActivated = activate;
        lastTimeActivated = Time.time;
        if (activate)
        {
            isActivated = true;
            onActivate.Invoke(pressedInfo);
        }
        else
        {
            isActivated = false;
            onDesactivate.Invoke(pressedInfo);
        }
    }

    private void OnCharDied(GameObject player, GameObject killer)
    {
        if(isCharActivating || isCharDesactivating)
        {
            if(charWhoActivate == player.GetComponent<ToricObject>().original)
            {
                isCharDying = true;
            }
        }
    }

    private bool TryGetCharacterInteract(out GameObject charWhoPressed)
    {
        if(dontUseInputSystem)
        {
            return TryGetCharacterInteractNotInputSystem(out charWhoPressed);
        }
        else
        {
            return TryGetCharacterInteractInputSystem(out charWhoPressed);
        }

        bool TryGetCharacterInteractNotInputSystem(out GameObject charWhoPressed)
        {
            Collider2D[] charCols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitboxOffset, hitboxSize, 0f, charMask);
            List<GameObject> charInFront = new List<GameObject>(4);
            foreach (Collider2D col in charCols)
            {
                if (col.CompareTag("Char"))
                {
                    GameObject charGO = col.GetComponent<ToricObject>().original;
                    charInFront.Add(charGO);
                }
            }

            foreach (GameObject player in charInFront)
            {
                if(!charInFrontLastFrame.Contains(player))
                {
                    charWhoPressed = player;
                    return true;
                }
            }

            charInFrontLastFrame = charInFront;
            charWhoPressed = null;
            return false;
        }

        bool TryGetCharacterInteractInputSystem(out GameObject charWhoPressed)
        {
            Collider2D[] charCols = PhysicsToric.OverlapBoxAll((Vector2)transform.position + hitboxOffset, hitboxSize, 0f, charMask);
            foreach (Collider2D col in charCols)
            {
                if (col.CompareTag("Char"))
                {
                    GameObject charGO = col.GetComponent<ToricObject>().original;
                    CharacterInputs charInput = charGO.GetComponent<CharacterInputs>();
                    if (charInput.interactPressedDown)
                    {
                        CharacterController charMvt = charGO.GetComponent<CharacterController>();
                        if (charMvt.isGrounded)
                        {
                            charWhoPressed = charGO;
                            return true;
                        }
                    }
                }
            }
            charWhoPressed = null;
            return false;
        }
    }

    #region Destroy/OnValidate/Gizmos

    private void OnDestroy()
    {
        EventManager.instance.callbackOnPlayerDeath -= OnCharDied;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement -= OnCharDied;
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
        hitboxSize = new Vector2(Mathf.Max(hitboxSize.x, 0f), Mathf.Max(hitboxSize.y, 0f));
        minDurationBefore2Activation = Mathf.Max(0f, minDurationBefore2Activation);
        durationItTakesToActivate = Mathf.Max(0f, durationItTakesToActivate);
        durationItTakesToDesactivate = Mathf.Max(0f, durationItTakesToDesactivate);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Hitbox.GizmosDraw((Vector2)transform.position + hitboxOffset, hitboxSize, Color.green);
    }

#endif

    #endregion

    #region Struct

    public struct PressedInfo
    {
        public GameObject charWhoPressed;

        public PressedInfo(GameObject charWhoPressed)
        {
            this.charWhoPressed = charWhoPressed;
        }
    }

    #endregion
}
