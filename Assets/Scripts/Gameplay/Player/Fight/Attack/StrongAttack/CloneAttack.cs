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
    private bool isCloneRendererEnable = false;
    [HideInInspector] public bool isCloneAttackEnable = false;
    private List<CloneData> dataCache = new List<CloneData>();
    private bool disableRegisteringData = false;

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

        eventController.callBackEnterTimePortal += OnEnterTimePortal;
        eventController.callBackExitTimePortal += OnExitTimePortal;
    }

    protected void OnEnterTimePortal(TimePortal timePortal)
    {
        disableRegisteringData = true;
    }

    protected void OnExitTimePortal(TimePortal timePortal)
    {
        disableRegisteringData = false;
    }

    protected override void Update()
    {
        base.Update();
        if(!disableRegisteringData)
            AddData();
        ApplyCloneModif();
        HandleCloneAttack();
        RemoveCloneData();
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

        CloneData data = new CloneData(transform.position, rot, actionLastFrame, Time.time, attackData, attack, dash, originalCreateExplosionThisFrame, movement.side == -1);
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
        if (!isCloneAttackEnable || lstCloneDatas.Count <= 0 || Time.time - lstCloneDatas[0].time < latenessTime)
            return;
        dataCache.Clear();
        int index = 0;
        do
        {
            if(Time.time - lstCloneDatas[index].time >= latenessTime)
            {
                if(lstCloneDatas[index].madeADashThisFrame)
                {
                    cloneWeakAttack.activateCloneDash = true;
                }
                else if (lstCloneDatas[index].makeAnExplosionThisFrame)
                {
                    cloneWeakAttack.activateWallExplosion = true;
                    cloneWeakAttack.cloneExplosionPosition = (Vector2)lstCloneDatas[index].attackData[0];
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
        yield return Useful.GetWaitForSeconds(duration);
        isCloneAttackEnable = false;
    }

    private struct CloneData
    {
        public Vector2 position;
        public float rotationZ;//en deg
        public Action action;
        public float time;
        public object[] attackData;
        public bool madeADashThisFrame;
        public bool makeAnExplosionThisFrame;
        public bool flipRenderer;

        public CloneData(in Vector2 position, float rotationZ, Action action,  float time, object[] attackData, bool makeAnAttack, bool madeADash, bool createExplosion, bool flipRenderer)
        {
            this.position = position;
            this.rotationZ = rotationZ;
            this.action = action;
            this.time = time;
            this.attackData = attackData;
            this.makeAnExplosionThisFrame = createExplosion;
            this.madeADashThisFrame = madeADash;
            this.flipRenderer = flipRenderer;
        }
    }
}
