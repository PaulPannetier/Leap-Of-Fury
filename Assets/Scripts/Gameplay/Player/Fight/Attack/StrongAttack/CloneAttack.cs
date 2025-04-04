using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class CloneAttack : StrongAttack
{
    private CharacterController movement;
    private List<CloneData> lstCloneDatas;
    private Action actionLastFrame;
    private GameObject clone;
    private Animator cloneAnimator;
    private SpriteRenderer cloneRenderer;
    private AmericanFistAttack cloneWeakAttack, weakAttack;
    private FightController fightController;
    [HideInInspector] public bool isCloneAttackEnable = false;
    private LayerMask charMask;
    private List<uint> charAlreadyToucheByDash;

    public GameObject clonePrefabs;
    [SerializeField] private float latenessTime = 3f;
    [SerializeField] private float duration = 5f;
    [Range(0f, 1f)] public float cloneTransparency = 0.4f;

    [HideInInspector] public bool originalCreateExplosionThisFrame;
    [HideInInspector] public Vector2 originalExplosionPosition;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<CharacterController>();
        fightController = GetComponent<FightController>();
        charAlreadyToucheByDash = new List<uint>(4);
        weakAttack = GetComponent<AmericanFistAttack>();
    }

    protected override void Start()
    {
        base.Start();
        lstCloneDatas = new List<CloneData>(duration.Floor() * SettingsManager.instance.currentConfig.targetedFPS.value.Floor());
        actionLastFrame = new Action(() => { });
        clone = Instantiate(clonePrefabs, transform.position, Quaternion.identity, transform);
        cloneAnimator = clone.GetComponent<Animator>();
        GetComponent<ToricObject>().chidrenToRemoveInClone.Add(clone);
        cloneRenderer = clone.GetComponent<SpriteRenderer>();
        cloneRenderer.enabled = false;
        StartCoroutine(EnableCloneRenderer());
        cloneWeakAttack = clone.GetComponent<AmericanFistAttack>();
        cloneWeakAttack.original = gameObject;
        cloneWeakAttack.originalCloneAttack = this;
        charMask = LayerMask.GetMask("Char");

        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
    }

    protected void LateUpdate()
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

        AddData();
        ApplyCloneModif();
        HandleCloneAttack();

        while (Time.time - lstCloneDatas[0].time >= latenessTime)
        {
            lstCloneDatas.RemoveAt(0);
        }
    }

    private void AddData()
    {
        float rot = transform.rotation.eulerAngles.z;
        bool createExplosion = false;
        bool weakAttackKillWithDash = weakAttack.isOriginalKillWithDash;
        Vector2? explosionPosition = null;

        if(originalCreateExplosionThisFrame)
        {
            createExplosion = true;
            explosionPosition = transform.position;
        }

        CloneData data = new CloneData(transform.position, rot, actionLastFrame, Time.time, weakAttackKillWithDash, createExplosion, explosionPosition, movement.flip, fightController.IsDashKillEnable());
        lstCloneDatas.Add(data);
        originalCreateExplosionThisFrame = false;

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
        int index = 0;
        while(lstCloneDatas.Count > index && Time.time - lstCloneDatas[index].time >= latenessTime)
        {
            CloneData cloneData = lstCloneDatas[index];

            cloneWeakAttack.isCloneDashEnable = cloneData.isKillWithWeakAttack;
            if (cloneData.makeAnExplosionThisFrame)
            {
                cloneWeakAttack.activateWallExplosion = true;
                cloneWeakAttack.cloneExplosionPosition = (Vector2)cloneData.explosionPosition;
            }

            if (isCloneAttackEnable && cloneData.isDashKillEnable)
            {
                Vector2 center = (Vector2)clone.transform.position + fightController.dashHitboxOffset;
                Collider2D[] cols = PhysicsToric.OverlapBoxAll(center, fightController.dashHitboxSize, 0f, charMask);
                foreach (Collider2D col in cols)
                {
                    if (col.CompareTag("Char"))
                    {
                        GameObject player = col.GetComponent<ToricObject>().original;
                        PlayerCommon pc = player.GetComponent<PlayerCommon>();
                        if (playerCommon.id != pc.id && !charAlreadyToucheByDash.Contains(pc.id))
                        {
                            base.OnTouchEnemy(player, damageType);
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
        cloneAnimator.enabled = false;
    }

    private void OnPauseDisable()
    {
        cloneAnimator.enabled = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
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

        yield return PauseManager.instance.Wait(duration);

        isCloneAttackEnable = false;
    }

    private IEnumerator EnableCloneRenderer()
    {
        yield return PauseManager.instance.Wait(latenessTime);

        cloneRenderer.enabled = true;
    }

#if UNITY_EDITOR

    protected override void SaveAttackStats()
    {
        GameStatisticManager.SetStat("char1WeakAttackDuration", duration.Round(2).ToString());
    }

#endif

    #region CloneData struct

    private struct CloneData
    {
        private byte boolUnion;
        public Vector2 position;
        public float rotationZ; //in deg
        public Action action;
        public float time;
        public Vector2? explosionPosition;

        public bool isKillWithWeakAttack => (boolUnion & 1) != 0; //Do an american fist attaque dashing
        public bool makeAnExplosionThisFrame => (boolUnion & (1 << 1)) != 0;
        public bool flipRenderer => (boolUnion & (1 << 2)) != 0;
        public bool isDashKillEnable => (boolUnion & (1 << 3)) != 0;

        public CloneData(in Vector2 position, float rotationZ, Action action, float time, bool isKillWithWeakAttack, bool createExplosion, in Vector2? explosionPosition, bool flipRenderer, bool isDashKillEnable)
        {
            this.position = position;
            this.rotationZ = rotationZ;
            this.action = action;
            this.time = time;
            boolUnion = 0;
            boolUnion = (byte)(isKillWithWeakAttack ? (boolUnion | 1) : boolUnion);
            boolUnion = (byte)(createExplosion ? (boolUnion | (1 << 1)) : boolUnion);
            this.explosionPosition = explosionPosition;
            boolUnion = (byte)(flipRenderer ? (boolUnion | (1 << 2)) : boolUnion);
            boolUnion = (byte)(isDashKillEnable ? (boolUnion | (1 << 3)) : boolUnion);
        }
    }

    #endregion

}
