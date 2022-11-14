using System;
using System.Collections.Generic;
using UnityEngine;

public class ToricObject : MonoBehaviour
{
    private static Bounds[] cameraBounds = new Bounds[4]
    {
            new Bounds(new Vector3(0f, PhysicsToric.cameraSize.y), PhysicsToric.cameraSize.ToVector3()),//haut
            new Bounds(new Vector3(0f, -PhysicsToric.cameraSize.y), PhysicsToric.cameraSize.ToVector3()),//bas
            new Bounds(new Vector3(PhysicsToric.cameraSize.x, 0f), PhysicsToric.cameraSize.ToVector3()),//droite
            new Bounds(new Vector3(-PhysicsToric.cameraSize.x, 0f), PhysicsToric.cameraSize.ToVector3()) //gauche
    };
    private static Vector2[] camOffsets;
    private static int[] invertCamOffsetIndex = new int[4] { 1, 0, 3, 2 };

    [SerializeField] private Bounds bounds;
    [SerializeField] private Vector2 boundsOffset;
    public List<Component> componentsToDisableInClone;
    public List<GameObject> chidrenToRemoveInClone;
    [SerializeField] private bool enableHorizontal = true, enableVertical = true;
    [HideInInspector] public bool isAClone;
    public GameObject original => isAClone ? cloner : gameObject;

    [HideInInspector] public List<ObjectClone> lstClones;
    [HideInInspector] public GameObject cloner;
    private bool[] oldCollideCamBounds = new bool[4];

    private void Start()
    {
        if(camOffsets == null)
        {
            camOffsets = new Vector2[4]
            {
                new Vector2(0f, -PhysicsToric.cameraSize.y),
                new Vector2(0f, PhysicsToric.cameraSize.y),
                new Vector2(-PhysicsToric.cameraSize.x, 0f),
                new Vector2(PhysicsToric.cameraSize.x, 0f)
            };
        }
        if(lstClones == null)
            lstClones = new List<ObjectClone>();
    }

    /// <summary>
    /// Applique la fonction en param du type de script en param aux clone/GO original
    /// </summary>
    public void ApplyToOther<T>(string methodName, in float delay = 0f) where T : MonoBehaviour
    {
        if(isAClone)
        {
            //on applique la fonction à l'original
            MonoBehaviour comp = cloner.GetComponent<T>();
            if(comp != null)
            {
                comp.Invoke(methodName, delay);
            }
        }
        else
        {
            //on applique la fonction à tout les clones
            foreach(ObjectClone clone in lstClones)
            {
                MonoBehaviour comp = clone.go.GetComponent<T>();
                comp.Invoke(methodName, delay);
            }
        }
    }

    private bool CollideWithCamBounds(in int index, out Vector2 offset)
    {
        if(bounds.Intersects(cameraBounds[index]))
        {
            offset = camOffsets[index];
            return true;
        }
        offset = Vector2.zero;
        return false;
    }

    private void Update()
    {
        if (isAClone)
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
                lstClones.Add(clone);
            }
        }

        //Update des clones
        foreach (ObjectClone clone in lstClones)
        {
            clone.go.transform.SetPositionAndRotation(transform.position + clone.offset, transform.rotation);
            clone.go.transform.localScale = transform.localScale;
        }

        //Update qui est le GO originel
        for (int i = beg; i < end; i++)
        {
            if (oldCollideCamBounds[i] && !collideWithCamBounds[i])
            {
                //On suppr le iéme clone
                foreach (ObjectClone clone in lstClones)
                {
                    if(clone.boundsIndex == i)
                    {
                        Destroy(clone.go);
                        lstClones.Remove(clone);
                        break;
                    }
                }
            }

            if(collideWithCamBounds[i] && cameraBounds[i].Contains(bounds.center))
            {
                //On switch le clone est l'original
                foreach (ObjectClone clone in lstClones)
                {
                    if (clone.boundsIndex == i)
                    {
                        Vector3 tmpPos = transform.position, tmpScale = transform.localScale;
                        Quaternion tmpRot = transform.rotation;
                        transform.SetPositionAndRotation(transform.position + clone.offset, clone.go.transform.rotation);
                        transform.localScale = clone.go.transform.localScale;

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

    private void RemoveClone(ObjectClone clone)
    {
        lstClones.Remove(clone);
        Destroy(clone.go);
    }

    public void RemoveClones()
    {
        for (int i = lstClones.Count - 1; i >= 0; i--)
        {
            RemoveClone(lstClones[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + boundsOffset.ToVector3(), bounds.size);

        Gizmos.DrawWireCube(Vector3.zero, PhysicsToric.cameraSize);
    }

    [Serializable]
    public class ObjectClone
    {
        public GameObject go;
        public Vector3 offset;
        public int boundsIndex;
        public ToricObject toricObject;

        public ObjectClone(GameObject clone, GameObject cloner, in Vector3 offset, in int boundsIndex)
        {
            this.go = clone;
            this.offset = offset;
            this.boundsIndex = boundsIndex;
            toricObject = clone.GetComponent<ToricObject>();
            toricObject.isAClone = true;
            toricObject.cloner = cloner;
        }
    }
}
