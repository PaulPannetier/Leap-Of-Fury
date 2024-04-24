#if UNITY_EDITOR

using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private bool enableUp;
    [SerializeField] private float speed;

    private void Start()
    {
        EventManager.instance.callbackPreUpdate += PreUpdate;
    }

    private void PreUpdate()
    {
        Vector2 velocity = (enableUp ? Vector2.up : Vector2.right) * speed;
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    private void OnDestroy()
    {
        EventManager.instance.callbackPreUpdate -= PreUpdate;
    }
}

#endif
