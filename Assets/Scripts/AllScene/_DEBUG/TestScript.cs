#if UNITY_EDITOR

using UnityEngine;

public class TestScript : MonoBehaviour
{
    private MapColliderData mapColliderData;

    public float speed = 5f;

    private void Awake()
    {
        mapColliderData = GetComponent<MapColliderData>();
    }

    private void Start()
    {
        PhysicsToric.AddPriorityCollider(GetComponent<BoxCollider2D>());
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
    }
}

#endif
