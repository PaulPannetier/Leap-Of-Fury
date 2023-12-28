using System;
using System.Collections.Generic;
using UnityEngine;
using Collision2D;
using System.Collections;
using System.Reflection;

public class ToricObject : MonoBehaviour
{
    private static Bounds[] mapBounds;
    private static Vector2[] camOffsets;
    private static int[] invertCamOffsetIndex = new int[4] { 1, 0, 3, 2 };

    private bool[] oldCollideCamBounds = new bool[4];
    private new Transform transform;

    [SerializeField] private Vector2 boundsOffset;
    [SerializeField] private bool enableHorizontal = true, enableVertical = true;

    [HideInInspector] public bool isAClone;
    [HideInInspector] public List<ObjectClone> clones;
    [HideInInspector] public GameObject cloner;
    [HideInInspector] public Action<Vector2, Vector2> onTeleportCallback;

    public List<Component> componentsToDisableInClone;
    public List<Component> componentsToSynchroniseInClone;
    public List<GameObject> chidrenToRemoveInClone;
    public GameObject original => isAClone ? cloner : gameObject;
    public Bounds bounds;

#if UNITY_EDITOR

    [SerializeField] private bool drawGizmos = true;

#endif

    #region Awake/Start

    private void Awake()
    {
        onTeleportCallback = (Vector2 newPos, Vector2 oldPos) => { };
        LevelMapData.onMapChange += OnMapChange;
        this.transform = base.transform;
    }

    private void Start()
    {
        if(clones == null)
            clones = new List<ObjectClone>();

        PauseManager.instance.callBackOnPauseDisable += OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable += OnPauseEnable;
    }

    #endregion

    #region OnMapChange

    private void OnMapChange(LevelMapData mapData)
    {
        camOffsets = new Vector2[4]
        {
            new Vector2(0f, -mapData.mapSize.y),
            new Vector2(0f, mapData.mapSize.y),
            new Vector2(-mapData.mapSize.x, 0f),
            new Vector2(mapData.mapSize.x, 0f)
        };

        mapBounds = new Bounds[4]
        {
            new Bounds(new Vector3(0f, mapData.mapSize.y), mapData.mapSize.ToVector3()),//haut
            new Bounds(new Vector3(0f, -mapData.mapSize.y), mapData.mapSize.ToVector3()),//bas
            new Bounds(new Vector3(mapData.mapSize.x, 0f), mapData.mapSize.ToVector3()),//droite
            new Bounds(new Vector3(-mapData.mapSize.x, 0f), mapData.mapSize.ToVector3()) //gauche
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
            //on applique la fonction à l'original
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
        float timeCounter = 0f;
        while (timeCounter < delay)
        {
            yield return null;
            if (!PauseManager.instance.isPauseEnable)
            {
                timeCounter += Time.deltaTime;
            }
        }

        comp.Invoke(methodName, 0f);
    }

    private void SynchComponent<T>(T comp) where T : Component
    {
        foreach (ObjectClone clone in clones)
        {
            T cloneComp = (T)clone.go.GetComponent(comp.GetType());
            Type type = cloneComp.GetType();

            FieldInfo[] fields = type.GetFields();
            PropertyInfo[] properties = type.GetProperties();

            foreach (FieldInfo fieldInfo in fields)
            {
                if(fieldInfo.IsPublic || fieldInfo.GetCustomAttribute<SerializeField>() != null)
                {
                    object value = fieldInfo.GetValue(comp);
                    fieldInfo.SetValue(cloneComp, value);
                }
            }

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.CanWrite && propertyInfo.CanRead || propertyInfo.GetCustomAttribute<SerializeField>() != null)
                {
                    object value = propertyInfo.GetValue(comp);
                    propertyInfo.SetValue(cloneComp, value);
                }
            }
        }
    }

    #endregion

    private bool CollideWithCamBounds(int index, out Vector2 offset)
    {
        if(bounds.Intersects(mapBounds[index]))
        {
            offset = camOffsets[index];
            return true;
        }
        offset = Vector2.zero;
        return false;
    }

    #region Update

    private void Update()
    {
        if (isAClone)
            return;

        if (PauseManager.instance.isPauseEnable)
            return;

        bounds.center = transform.position + boundsOffset.ToVector3();

        bool[] collideWithCamBounds = new bool[4];

        int beg = enableVertical ? 0 : enableHorizontal ? 2 : 4;
        int end = enableHorizontal ? 4 : enableVertical ? 2 : 0;

        for (int i = beg; i < end; i++)
        {
            collideWithCamBounds[i] = CollideWithCamBounds(i, out Vector2 offset);
            if(collideWithCamBounds[i] && !oldCollideCamBounds[i])
            {
                GameObject tmpGO = Instantiate(gameObject, CloneParent.cloneParent);
                ObjectClone clone = new ObjectClone(tmpGO, gameObject, offset, i);
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
            }
        }

        //Update clones
        foreach (ObjectClone clone in clones)
        {
            clone.go.transform.SetPositionAndRotation(transform.position + clone.offset, transform.rotation);
            clone.go.transform.localScale = transform.localScale;

            foreach(Component comp in componentsToSynchroniseInClone)
            {
                string mess = comp.GetType().Name;
                SynchComponent(comp);
            }
        }

        //Update qui est le GO originel
        for (int i = beg; i < end; i++)
        {
            if (oldCollideCamBounds[i] && !collideWithCamBounds[i])
            {
                //On suppr le iéme clone
                foreach (ObjectClone clone in clones)
                {
                    if(clone.boundsIndex == i)
                    {
                        Destroy(clone.go);
                        clones.Remove(clone);
                        break;
                    }
                }
            }

            if(collideWithCamBounds[i] && mapBounds[i].Contains(bounds.center))
            {
                //On switch le clone est l'original
                foreach (ObjectClone clone in clones)
                {
                    if (clone.boundsIndex == i)
                    {
                        Vector3 tmpPos = transform.position, tmpScale = transform.localScale;
                        Quaternion tmpRot = transform.rotation;
                        Vector2 newPos = transform.position + clone.offset;
                        transform.SetPositionAndRotation(newPos, clone.go.transform.rotation);
                        transform.localScale = clone.go.transform.localScale;

                        onTeleportCallback.Invoke(newPos, tmpPos);

                        clone.go.transform.position = tmpPos;
                        clone.go.transform.rotation = tmpRot;
                        clone.go.transform.localScale = tmpScale;
                        clone.offset *= -1f;
                        clone.boundsIndex = invertCamOffsetIndex[clone.boundsIndex];

                        bool[] tmp = (bool[])collideWithCamBounds.Clone();
                        for (int j = 0; j < 4; j++)
                            collideWithCamBounds[j] = tmp[invertCamOffsetIndex[j]];

                        break;
                    }
                }
            }
            /*
            if (collideWithCamBounds[i] && cameraBounds[i].Contain(bounds))
            {
                //On devient le iéme clone et on le détruit
                foreach (ObjectClone clone in lstClones)
                {
                    if (clone.boundsIndex == i)
                    {
                        transform.SetPositionAndRotation(transform.position + clone.offset, transform.rotation);
                        transform.localScale = clone.go.transform.localScale;
                        RemoveClone(clone);
                        break;
                    }
                }
            }
            */
        }

        oldCollideCamBounds = collideWithCamBounds;
    }

    #endregion

    #region RemoveClone

    private void RemoveClone(ObjectClone clone)
    {
        clones.Remove(clone);
        Destroy(clone.go);
    }

    public void RemoveClones()
    {
        for (int i = clones.Count - 1; i >= 0; i--)
        {
            RemoveClone(clones[i]);
        }
    }

    private void OnDestroy()
    {
        LevelMapData.onMapChange -= OnMapChange;
        PauseManager.instance.callBackOnPauseDisable -= OnPauseDisable;
        PauseManager.instance.callBackOnPauseEnable -= OnPauseEnable;
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

        Gizmos.color = Color.red;
        Hitbox.GizmosDraw((Vector2)transform.position + boundsOffset, bounds.size);
        Hitbox.GizmosDraw(Vector3.zero, LevelMapData.currentMap.mapSize);
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
        public Vector3 offset;
        public int boundsIndex;
        public ToricObject toricObject;
        public Animator animator;

        public ObjectClone(GameObject clone, GameObject cloner, in Vector3 offset, in int boundsIndex)
        {
            this.go = clone;
            this.offset = offset;
            this.boundsIndex = boundsIndex;
            toricObject = clone.GetComponent<ToricObject>();
            toricObject.isAClone = true;
            toricObject.cloner = cloner;
            animator = clone.GetComponent<Animator>();
        }
    }

    #endregion
}
