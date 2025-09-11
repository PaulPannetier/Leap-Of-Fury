using System;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using System.Collections;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public class ToricObject : MonoBehaviour
{
    #region fields

    private static Hitbox[] mapsHitboxesAround;
    private static Vector2[] camOffsets;
    private static short[] invertCamOffsetIndex = new short[4] { 1, 0, 3, 2 };

    private bool[] oldCollideCamBounds = new bool[4];
    private new Transform transform;
    private List<ObjectClone> clones;
    private Hitbox currentHitbox;

    [SerializeField] private Vector2 boundsOffset;
    public Vector2 boundsSize;
    [SerializeField] private bool enableHorizontal = true, enableVertical = true;

    [HideInInspector] public bool isAClone;
    [HideInInspector] public GameObject cloner {  get; private set; }

    [SerializeField] private List<Component> componentsToDisableInClone;
    [SerializeField] private List<Component> componentsToSynchroniseInClone;
    public List<GameObject> chidrenToRemoveInClone;

    [Tooltip("Allow to call the update methods externally (via other script).")] public bool useCustomUpdate = false;

    public GameObject original => isAClone ? cloner : gameObject;
    public Action<Vector2, Vector2> onTeleportCallback;
    public Action<GameObject> onCloneCreatedCallback;
    public Action<GameObject> onCloneDestroyCallback;

#if UNITY_EDITOR
    [SerializeField] private bool drawGizmos = true;
#endif

    #endregion

    #region Awake/Start

    private void Awake()
    {
        onTeleportCallback = (Vector2 pos, Vector2 oldPos) => { };
        onCloneCreatedCallback = (GameObject clone) => { };
        onCloneDestroyCallback = (GameObject clone) => { };
        EventManager.instance.callbackOnMapChanged += OnMapChange;
        clones = new List<ObjectClone>(4);
        this.transform = base.transform;
    }

    private void Start()
    {
        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
    }

    #endregion

    #region OnMapChange

    private void OnMapChange(LevelMapData mapData)
    {
        if (isAClone)
            return;

        Vector2 mapSize = mapData.mapSize * mapData.cellSize;
        camOffsets = new Vector2[4]
        {
            new Vector2(0f, -mapSize.y),
            new Vector2(0f, mapSize.y),
            new Vector2(-mapSize.x, 0f),
            new Vector2(mapSize.x, 0f)
        };

        mapsHitboxesAround = new Hitbox[4]
        {
            new Hitbox(new Vector2(0f, mapSize.y), mapSize),
            new Hitbox(new Vector2(0f, -mapSize.y), mapSize),
            new Hitbox(new Vector2(mapSize.x, 0f), mapSize),
            new Hitbox(new Vector2(-mapSize.x, 0f), mapSize)
        };
    }

    #endregion

    #region Synchronisation methods

    /// <summary>
    /// Applique la fonction en param du type de script en param aux clone/GO original
    /// </summary>
    public void ApplyToOther<T>(string methodName, float delay = 0f) where T : MonoBehaviour
    {
        if(isAClone)
        {
            //on applique la fonction a l'original
            MonoBehaviour comp = cloner.GetComponent<T>();
            if(comp != null)
            {
                StartCoroutine(InvokePause(comp, methodName, delay));
            }
        }
        else
        {
            //Apply the method for all clones
            foreach(ObjectClone clone in clones)
            {
                MonoBehaviour comp = clone.go.GetComponent<T>();
                StartCoroutine(InvokePause(comp, methodName, delay));
            }
        }
    }

    private IEnumerator InvokePause(MonoBehaviour comp,  string methodName, float delay)
    {
        yield return PauseManager.instance.Wait(delay);

        comp.Invoke(methodName, 0f);
    }

    private void SynchComponent<T>(T comp) where T : Component
    {
        if(comp is Animator animator)
        {
            SynchAnimator(animator);
            return;
        }

        if (comp is SpriteRenderer sr)
        {
            SynchSpriteRenderer(sr);
            return;
        }

        foreach (ObjectClone clone in clones)
        {
            T cloneComp = (T)clone.go.GetComponent(comp.GetType());
            Type type = cloneComp.GetType();

            FieldInfo[] fields = type.GetFields();
            PropertyInfo[] properties = type.GetProperties();

            foreach (FieldInfo field in fields)
            {
                if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
                {
                    object value = field.GetValue(comp);
                    field.SetValue(cloneComp, value);
                }
            }

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.CanWrite && propertyInfo.CanRead)
                {
                    object value = propertyInfo.GetValue(comp);
                    propertyInfo.SetValue(cloneComp, value);
                }
            }
        }
    }

    private void SynchSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        foreach (ObjectClone clone in clones)
        {
            SpriteRenderer cloneRenderer = clone.go.GetComponent<SpriteRenderer>();
            if(cloneRenderer == null)
                continue;

            cloneRenderer.sprite = spriteRenderer.sprite;
            cloneRenderer.color = spriteRenderer.color;
            cloneRenderer.flipX = spriteRenderer.flipX;
            cloneRenderer.flipY = spriteRenderer.flipY;
            cloneRenderer.drawMode = spriteRenderer.drawMode;
            cloneRenderer.maskInteraction = spriteRenderer.maskInteraction;
            cloneRenderer.spriteSortPoint = spriteRenderer.spriteSortPoint;
            cloneRenderer.material = spriteRenderer.material;
            cloneRenderer.sortingOrder = spriteRenderer.sortingOrder;
            cloneRenderer.renderingLayerMask = spriteRenderer.renderingLayerMask;
            cloneRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            cloneRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        }
    }

    private void SynchAnimator(Animator animator)
    {
        foreach (ObjectClone clone in clones)
        {
            if(clone.animator != null)
            {
                foreach (AnimatorControllerParameter animParam in animator.parameters)
                {
                    switch (animParam.type)
                    {
                        case AnimatorControllerParameterType.Float:
                            clone.animator.SetFloat(animParam.nameHash, animator.GetFloat(animParam.nameHash));
                            break;
                        case AnimatorControllerParameterType.Int:
                            clone.animator.SetInteger(animParam.nameHash, animator.GetInteger(animParam.nameHash));
                            break;
                        case AnimatorControllerParameterType.Bool:
                            clone.animator.SetBool(animParam.nameHash, animator.GetBool(animParam.nameHash));
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    #endregion

    #region Update

    public void CustomUpdate()
    {
        UpdateInternal();
    }

    private void Update()
    {
        if (isAClone || useCustomUpdate)
            return;

        UpdateInternal();
    }

    private void UpdateInternal()
    {
        if (PauseManager.instance.isPauseEnable)
            return;

        currentHitbox = new Hitbox((Vector2)transform.position + boundsOffset, boundsSize);

        bool[] collideWithCamBounds = new bool[4];

        int beg = enableVertical ? 0 : enableHorizontal ? 2 : 4;
        int end = enableHorizontal ? 4 : enableVertical ? 2 : 0;

        for (int i = beg; i < end; i++)
        {
            if (oldCollideCamBounds[i] && mapsHitboxesAround[i].Contains(currentHitbox.center))
            {
                //On switch le clone est l'original
                foreach (ObjectClone clone in clones)
                {
                    if (clone.boundsIndex == i)
                    {
                        Vector3 oldPos = transform.position, tmpScale = transform.localScale;
                        Quaternion tmpRot = transform.rotation;
                        Vector2 newPos = (Vector2)transform.position + clone.offset;
                        transform.SetPositionAndRotation(newPos, clone.go.transform.rotation);
                        transform.localScale = clone.go.transform.localScale;
                        currentHitbox.MoveAt(newPos + boundsOffset);

                        onTeleportCallback.Invoke(newPos, oldPos);

                        clone.go.transform.position = oldPos;
                        clone.go.transform.rotation = tmpRot;
                        clone.go.transform.SetPositionAndRotation(oldPos, tmpRot);
                        clone.go.transform.localScale = tmpScale;
                        clone.offset *= -1f;
                        clone.boundsIndex = invertCamOffsetIndex[clone.boundsIndex];

                        bool[] tmp = (bool[])oldCollideCamBounds.Clone();
                        for (int j = 0; j < 4; j++)
                        {
                            oldCollideCamBounds[j] = tmp[invertCamOffsetIndex[j]];
                        }
                        break;
                    }
                }
                break;
            }
        }

        for (int i = beg; i < end; i++)
        {
            collideWithCamBounds[i] = mapsHitboxesAround[i].Collide(currentHitbox);
            if(collideWithCamBounds[i] && !oldCollideCamBounds[i])
            {
                GameObject tmpGO = Instantiate(gameObject, CloneParent.cloneParent);
                ObjectClone clone = new ObjectClone(tmpGO, gameObject, camOffsets[i], i);
                foreach(Component component in clone.toricObject.componentsToDisableInClone)
                {
                    if (component is MonoBehaviour m)
                        m.enabled = false;
                    else
                        Destroy(component);
                }

                foreach(GameObject go in clone.toricObject.chidrenToRemoveInClone)
                {
                    go.SetActive(false);
                    Destroy(go);
                }
                clones.Add(clone);
                onCloneCreatedCallback.Invoke(clone.go);
            }
        }

        for (int i = beg; i < end; i++)
        {
            if (oldCollideCamBounds[i] && !collideWithCamBounds[i])
            {
                //Remove ieme clone
                foreach (ObjectClone clone in clones)
                {
                    if (clone.boundsIndex == i)
                    {
                        RemoveClone(clone);
                        break;
                    }
                }
            }
        }

        //Update clones
        foreach (ObjectClone clone in clones)
        {
            clone.go.transform.SetPositionAndRotation((Vector2)transform.position + clone.offset, transform.rotation);
            clone.go.transform.localScale = transform.localScale;

            foreach (Component comp in componentsToSynchroniseInClone)
            {
                SynchComponent(comp);
            }
        }

        oldCollideCamBounds = collideWithCamBounds;

        //Anti bug

        int maxClone = enableHorizontal ? (enableVertical ? 3 : 1) : (enableVertical ? 1 : 0);
        if(clones.Count > maxClone)
        {
#if false && (UNITY_EDITOR || ADVANCE_DEBUG)
            LogManager.instance.WriteLog($"The number of clones of the GO cannot exceed {maxClone} but reach {clones.Count}", clones, maxClone, transform.position, gameObject);
#endif
            RemoveClones();
            transform.position = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + boundsOffset) - boundsOffset;
            print("Debug pls");
        }

        //Anti bug 2
        if (clones.Count <= 0 && !PhysicsToric.IsPointInsideBound((Vector2)transform.position + boundsOffset))
        {
            transform.position = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + boundsOffset) - boundsOffset;
            print("Debug pls");
        }

        //Anti bug 3
        if (clones.Count > 0)
        {
            bool allClonesIsOutBounded = true;
            foreach (ObjectClone clone in clones)
            {
                if(PhysicsToric.IsPointInsideBound((Vector2)clone.go.transform.position + boundsOffset))
                {
                    allClonesIsOutBounded = false;
                    break;
                }
            }

            if(allClonesIsOutBounded && !PhysicsToric.IsPointInsideBound((Vector2)transform.position + boundsOffset))
            {
                RemoveClones();
                transform.position = PhysicsToric.GetPointInsideBounds((Vector2)transform.position + boundsOffset) - boundsOffset;
                print("Debug pls");
            }
        }
    }

    #endregion

    #region RemoveClone

    private void RemoveClone(ToricObject toClone)
    {
        for (int i = clones.Count - 1; i >= 0; i--)
        {
            if (clones[i].toricObject == toClone)
            {
                RemoveClone(clones[i]);
            }
        }
    }

    private void RemoveClone(ObjectClone clone)
    {
        clones.Remove(clone);
        onCloneDestroyCallback.Invoke(clone.go);
        Destroy(clone.go);
    }

    private void RemoveClones()
    {
        for (int i = clones.Count - 1; i >= 0; i--)
        {
            RemoveClone(clones[i]);
        }
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackOnMapChanged -= OnMapChange;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;

        if(!isAClone)
        {
            RemoveClones();
        }
    }

    #endregion

    #region Gizmos/OnValidate/Pause

    private void OnPauseEnable()
    {
        foreach (ObjectClone clone in clones)
        {
            if(clone.animator != null)
            {
                clone.animator.speed = 0f;
            }
        }
    }

    private void OnPauseDisable()
    {
        foreach (ObjectClone clone in clones)
        {
            if (clone.animator != null)
            {
                clone.animator.speed = 1f;
            }
        }
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if(!drawGizmos)
            return;

        Hitbox.GizmosDraw((Vector2)transform.position + boundsOffset, boundsSize, Color.red);

        if(PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            Hitbox.GizmosDraw(Vector2.zero, LevelMapData.currentMap.mapSize * LevelMapData.currentMap.cellSize, Color.red);
        }
    }

    private void OnValidate()
    {
        this.transform = base.transform;
    }

#endif

    #endregion

    #region Class

    [Serializable]
    public class ObjectClone
    {
        public GameObject go;
        public Vector2 offset;
        public int boundsIndex;
        public ToricObject toricObject;
        public Animator animator;

        public ObjectClone(GameObject clone, GameObject cloner, in Vector2 offset, int boundsIndex)
        {
            this.go = clone;
            this.offset = offset;
            this.boundsIndex = boundsIndex;
            toricObject = clone.GetComponent<ToricObject>();
#if UNITY_EDITOR
            toricObject.drawGizmos = false;
#endif
            toricObject.isAClone = true;
            toricObject.cloner = cloner;
            animator = clone.GetComponent<Animator>();
        }
    }

    #endregion
}
