using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CloneAttack : StrongAttack
{
    private Movement movement;
    private List<CloneData> lstCloneDatas;
    private Action actionLastFrame;
    private GameObject clone;
    private Animator cloneAnimator;
    private SpriteRenderer cloneRenderer;
    private AmericanFistAttack cloneWeakAttack;
    private FightController fightController;
    private bool isCloneRendererEnable = false;
    [HideInInspector] public bool isCloneAttackEnable = false;
    private bool disableRegisteringData, pauseCloneFollow;
    private LayerMask charMask;
    private List<uint> charAlreadyToucheByDash;

    public GameObject clonePrefabs;
    [SerializeField] private float latenessTime = 3f;
    [SerializeField] private float duration = 5f;
    [Range(0f, 1f)] public float cloneTransparency = 0.4f;

    [HideInInspector] public bool originalDashThisFrame;
    [HideInInspector] public bool originalCreateExplosionThisFrame;
    [HideInInspector] public Vector2 originalExplosionPosition;

    public new float attackForce => cloneWeakAttack.attackForce;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
        fightController = GetComponent<FightController>();
        charAlreadyToucheByDash = new List<uint>();
    }

    protected override void Start()
    {
        base.Start();
        lstCloneDatas = new List<CloneData>();
        actionLastFrame = new Action(() => { });
        clone = Instantiate(clonePrefabs, transform.position, Quaternion.identity, transform);
        cloneAnimator = clone.GetComponent<Animator>();
        GetComponent<ToricObject>().chidrenToRemoveInClone.Add(clone);
        cloneRenderer = clone.GetComponent<SpriteRenderer>();
        cloneWeakAttack = clone.GetComponent<AmericanFistAttack>();
        cloneWeakAttack.original = gameObject;
        cloneWeakAttack.originalCloneAttack = this;
        cloneRenderer.enabled = false;
        charMask = LayerMask.GetMask("Char");

        eventController.callBackEnterTimePortal += OnEnterTimePortal;
        eventController.callBackExitTimePortal += OnExitTimePortal;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
    }

    protected override void Update()
    {
        base.Update();

        if(PauseManager.instance.isPauseEnable)
        {
            for (int i = 0; i < lstCloneDatas.Count; i++)
            {
                CloneData cloneData = lstCloneDatas[i];
                cloneData.time += Time.deltaTime;
                lstCloneDatas[i] = cloneData;
            }
            return;
        }

        if(!disableRegisteringData)
            AddData();

        if(pauseCloneFollow)
        {
            for (int i = 0; i < lstCloneDatas.Count; i++)
            {
                CloneData tmp = lstCloneDatas[i];
                tmp.time += Time.deltaTime;
                lstCloneDatas[i] = tmp;
            }
        }
        else
        {
            ApplyCloneModif();
            HandleCloneAttack();
            RemoveCloneData();
        }
    }

    private void AddData()
    {
        float rot = transform.rotation.eulerAngles.z * Mathf.Rad2Deg;
        object[] attackData = null;
        bool attack = false, dash = false;

        if (originalDashThisFrame)
        {
            attackData = new object[] { movement.GetCurrentDirection() };
            dash = true;
        }
        else if(originalCreateExplosionThisFrame)
        {
            attackData = new object[] { originalExplosionPosition };
            attack = true;
        }

        CloneData data = new CloneData(transform.position, rot, actionLastFrame, Time.time, attackData, attack, dash, originalCreateExplosionThisFrame, movement.side == -1, fightController.canKillDashing);
        lstCloneDatas.Add(data);
        originalDashThisFrame = originalCreateExplosionThisFrame = false;

        actionLastFrame = new Action(() => { });
    }

    private void ApplyCloneModif()
    {
        if (lstCloneDatas.Count <= 0)
            return;

        CloneData data = lstCloneDatas[0];
        clone.transform.SetPositionAndRotation(data.position, Quaternion.Euler(0f, 0f, data.rotationZ));
        cloneRenderer.flipX = data.flipRenderer;
        cloneRenderer.color = isCloneAttackEnable ? playerCommon.color : playerCommon.color * cloneTransparency;
        data.action.Invoke();
    }

    private void HandleCloneAttack()
    {
        if (lstCloneDatas.Count <= 0 || Time.time - lstCloneDatas[0].time < latenessTime)
            return;

        int index = 0;
        do
        {
            if(Time.time - lstCloneDatas[index].time >= latenessTime)
            {
                CloneData cloneData = lstCloneDatas[index];
                if(cloneData.madeADashThisFrame)
                {
                    cloneWeakAttack.activateCloneDash = true;
                }
                else if (cloneData.makeAnExplosionThisFrame)
                {
                    cloneWeakAttack.activateWallExplosion = true;
                    cloneWeakAttack.cloneExplosionPosition = (Vector2)lstCloneDatas[index].attackData[0];
                }

                if(cloneData.isDashKillEnable)
                {
                    Vector2 center = (Vector2)clone.transform.position + fightController.dashHitboxOffset;
                    Collider2D[] cols = PhysicsToric.OverlapBoxAll(center, fightController.dashHitboxSize, 0f, charMask);
                    foreach (Collider2D col in cols)
                    {
                        if(col.CompareTag("Char"))
                        {
                            GameObject player = col.GetComponent<ToricObject>().original;
                            PlayerCommon pc = player.GetComponent<PlayerCommon>();
                            if (playerCommon.id != pc.id)
                            {
                                base.OnTouchEnemy(player);
                                charAlreadyToucheByDash.Add(pc.id);
                            }
                        }
                    }
                }
                else
                {
                    charAlreadyToucheByDash.Clear();
                }
                index++;
            }
            else
            {
                break;
            }
        } while (true);
    }

    private void RemoveCloneData()
    {
        while(true)
        {
            if (lstCloneDatas.Count > 0 && Time.time - lstCloneDatas[0].time > latenessTime)
            {
                lstCloneDatas.RemoveAt(0);
                if (!isCloneRendererEnable)
                {
                    isCloneRendererEnable = cloneRenderer.enabled = true;
                }
            }
            else
            {
                break;
            }
        }
    }

    #region OnTrigger

    protected override void OnTriggerAnimatorSetFloat(string name, float value)
    {
        actionLastFrame += () =>
        {
            cloneAnimator.SetFloat(name, value);
        };
    }

    protected override void OnTriggerAnimatorSetBool(string name, bool value)
    {
        actionLastFrame += () =>
        {
            cloneAnimator.SetBool(name, value);
        };
    }

    protected override void OnTriggerAnimatorSetTrigger(string name)
    {
        actionLastFrame += () =>
        {
            cloneAnimator.SetTrigger(name);
        };
    }

    private void OnPauseEnable()
    {
        pauseCloneFollow = true;
        cloneAnimator.enabled = false;
    }

    private void OnPauseDisable()
    {
        pauseCloneFollow = false;
        cloneAnimator.enabled = true;
    }

    protected void OnEnterTimePortal(TimePortal timePortal)
    {
        disableRegisteringData = true;
    }

    protected void OnExitTimePortal(TimePortal timePortal)
    {
        disableRegisteringData = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        eventController.callBackEnterTimePortal -= OnEnterTimePortal;
        eventController.callBackExitTimePortal -= OnExitTimePortal;
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
    }

    #endregion

    public override bool Launch(Action callbackEnableOtherAttack, Action callbackEnableThisAttack)
    {
        if(!cooldown.isActive)
        {
            callbackEnableOtherAttack.Invoke();
            callbackEnableThisAttack.Invoke();
            return false;
        }

        base.Launch(callbackEnableOtherAttack, callbackEnableThisAttack);
        StartCoroutine(EnableCloneAttack());
        cooldown.Reset();
        callbackEnableOtherAttack.Invoke();
        callbackEnableThisAttack.Invoke();
        return true;
    }

    private IEnumerator EnableCloneAttack()
    {
        isCloneAttackEnable = true;

        float timeCounter = 0f;
        while (timeCounter < duration)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        isCloneAttackEnable = false;
    }

    #region CloneData struct

    private struct CloneData
    {
        public Vector2 position;
        public float rotationZ;//en deg
        public Action action;
        public float time;
        public object[] attackData;
        public bool madeADashThisFrame;
        public bool makeAnExplosionThisFrame;
        public bool isDashKillEnable;
        public bool flipRenderer;

        public CloneData(in Vector2 position, float rotationZ, Action action,  float time, object[] attackData, bool makeAnAttack, bool madeADash, bool createExplosion, bool flipRenderer, bool isDashKillEnable)
        {
            this.position = position;
            this.rotationZ = rotationZ;
            this.action = action;
            this.time = time;
            this.attackData = attackData;
            this.makeAnExplosionThisFrame = createExplosion;
            this.madeADashThisFrame = madeADash;
            this.flipRenderer = flipRenderer;
            this.isDashKillEnable = isDashKillEnable;
        }
    }

    #endregion
}
