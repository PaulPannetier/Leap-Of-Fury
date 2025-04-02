using UnityEngine;

public class ConeProjectile : MonoBehaviour
{
    private new Transform transform;
    private Vector2 dir;
    private float speed;

    private void Awake()
    {
        this.transform = base.transform;
    }

    public void Launch(float speed, in Vector2 dir, ConeProjectileAttack attack)
    {
        this.speed = speed;
        this.dir = dir;
    }

    private void Update()
    {
        transform.position += (Vector3)dir * speed * Time.deltaTime;
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        this.transform = base.transform;
    }

#endif

    #endregion
}
