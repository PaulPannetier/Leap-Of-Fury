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
    private GameObject charDied;
    private List<GameObject> charInFrontLastFrame;
    private bool isPauseEnable;

    public bool enableBehaviour = true;
#if UNITY_EDITOR
    public bool drawGizmos = true;
#endif
    [SerializeField] private Vector2 hitboxOffset, hitboxSize;
    [SerializeField] private bool allowDesactivation = true;
    [SerializeField] private float minDurationBeforeReactivation;
    [SerializeField] private float durationItTakesToActivate = 1f;
    [SerializeField] private float durationItTakesToDesactivate = 1f;
    [SerializeField] private float activationDuration = -1f;//unlimited if < 0f
    [SerializeField, Tooltip("Be triggered when a player pass througt the button")] private bool dontUseInputSystem = false;

    [HideInInspector] public bool isActivated { get; private set; }
    public PressedInfo pressedInfo { get; private set; }
    public Action<PressedInfo> onActivate;
    public Action onDesactivate;

    private void Awake()
    {
        onActivate = new Action<PressedInfo>((PressedInfo arg) => { });
        onDesactivate = new Action(() => { });
        charMask = LayerMask.GetMask("Char");
        this.transform = base.transform;
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        EventManager.instance.callbackOnPlayerDeath += OnCharDied;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement += OnCharDied;
        charInFrontLastFrame = new List<GameObject>();
    }

    private void Update()
    {
        GetComponentInChildren<SpriteRenderer>().color = isActivated ? Color.red : Color.green;

        if (!enableBehaviour || isPauseEnable)
            return;

        if (isCharActivating || isCharDesactivating)
            return;

        if (isActivated)
        {
            if((Time.time - lastTimeActivated > minDurationBeforeReactivation) || allowDesactivation)
            {
                if(TryGetCharacterInteract(out GameObject charWhoPressed))
                {
                    if(allowDesactivation)
                    {
                        Desactivate(charWhoPressed);
                    }
                    else
                    {
                        Activate(charWhoPressed);
                    }
                }
            }

            if(activationDuration > 0f && Time.time - lastTimeActivated > activationDuration)
            {
                Desactivate(null);
            }
        }
        else
        {
            if (TryGetCharacterInteract(out GameObject charWhoPressed))
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
        charDied = null;
        isCharActivating = activate;
        isCharDesactivating = !activate;
        float duration = activate ? durationItTakesToActivate : durationItTakesToDesactivate;
        float time = Time.time;
        bool cancel = false;
        CharacterController charMvt = pressedInfo.charWhoPressed.GetComponent<CharacterController>();
        charMvt.Freeze();
        while(Time.time - time < duration)
        {
            if(isCharDying)
            {
                if(charDied == pressedInfo.charWhoPressed)
                {
                    cancel = true;
                    isCharDying = false;
                    charDied = null;
                    break;
                }
                isCharDying = false;
                charDied = null;
            }

            float delta = Time.time - lastTimeActivated;
            while(isPauseEnable)
            {
                yield return null;
            }
            lastTimeActivated = Time.time - delta;

            yield return null;
        }
        charMvt.UnFreeze();

        if(!cancel)
        {
            OnActivate(activate, pressedInfo);
        }
        isCharActivating = isCharDesactivating = false;
    }

    private void OnActivate(bool activate, PressedInfo pressedInfo)
    {
        isActivated = activate;
        if (activate)
        {
            lastTimeActivated = Time.time;
            this.pressedInfo = pressedInfo;
            isActivated = true;
            onActivate.Invoke(pressedInfo);
        }
        else
        {
            this.pressedInfo = new PressedInfo(null); ;
            isActivated = false;
            onDesactivate.Invoke();
        }
    }

    private void OnCharDied(GameObject player, GameObject killer)
    {
        isCharDying = true;
        charDied = player.GetComponent<ToricObject>().original;
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
            List<GameObject> charInFront = new List<GameObject>();
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
                    charInFrontLastFrame = charInFront;
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

    #region Destroy/Pause/OnValidate/Gizmos

    private void OnDestroy()
    {
        EventManager.instance.callbackOnPlayerDeath -= OnCharDied;
        EventManager.instance.callbackOnPlayerDeathByEnvironnement -= OnCharDied;
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
    }

    private IEnumerator PauseCorout()
    {
        float delta = Time.time - lastTimeActivated;
        while(isPauseEnable)
        {
            yield return null;
        }
        lastTimeActivated = Time.time - delta;
    }

    private void OnPauseEnable()
    {
        isPauseEnable = true;
        StartCoroutine(PauseCorout());
    }

    private void OnPauseDisable()
    {
        isPauseEnable = false;
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        hitboxSize = new Vector2(Mathf.Max(hitboxSize.x, 0f), Mathf.Max(hitboxSize.y, 0f));
        this.transform = base.transform;
        minDurationBeforeReactivation = Mathf.Max(0f, minDurationBeforeReactivation);
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
