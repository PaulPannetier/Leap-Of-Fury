#if UNITY_EDITOR

using UnityEngine;

public class TestScript : MonoBehaviour
{
    private MapColliderData mapColliderData;

    [SerializeField] private bool enableUp;
    [SerializeField] private float speed;

    private void Start()
    {
        EventManager.instance.callbackPreUpdate += PreUpdate;
        mapColliderData = GetComponent<MapColliderData>();
    }

    private void PreUpdate()
    {
        mapColliderData.velocity = (enableUp ? Vector2.up : Vector2.right) * speed;
        transform.position += (Vector3)mapColliderData.velocity * Time.deltaTime;
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackPreUpdate -= PreUpdate;
    }
}

#endif
