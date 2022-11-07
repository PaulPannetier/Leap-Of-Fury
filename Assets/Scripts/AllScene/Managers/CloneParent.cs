using UnityEngine;

public class CloneParent : MonoBehaviour
{
    public static Transform cloneParent;

    private void Awake()
    {
        if(cloneParent != null)
        {
            Destroy(gameObject);
            return;
        }
        cloneParent = transform;
        DontDestroyOnLoad(gameObject);
    }
}
