using UnityEngine;

[RequireComponent(typeof(MapColliderData))]
public abstract class ConvoyerBelt : MonoBehaviour
{
    private MapColliderData mapColliderData;

    private bool _isActive;
    public bool isActive
    {
        get => _isActive;
        set
        {
            if (mapColliderData == null)
                mapColliderData = GetComponent<MapColliderData>();

            if (value)
            {
                mapColliderData.groundType = MapColliderData.GroundType.convoyerBelt;
                mapColliderData.grabable = false;
                mapColliderData.disableAntiKnockHead = true;
            }
            else
            {
                mapColliderData.groundType = MapColliderData.GroundType.normal;
                mapColliderData.grabable = true;
                mapColliderData.disableAntiKnockHead = false;
            }
            _isActive = value;
        }
    }

    [field:SerializeField] public bool startActive { get; protected set; } = true;
    public float maxSpeed;
    public float speedLerp;

    [Header("Use by Interruptor")]
    [SerializeField] private bool useByInterruptor;
    [SerializeField] private bool invertInterruptor;
    [SerializeField] private Interruptor interruptor;

    protected virtual void Awake()
    {
        mapColliderData = GetComponent<MapColliderData>();
        isActive = startActive;
    }

    protected virtual void Update()
    {
        if(useByInterruptor)
        {
            if(isActive)
            {
                if(!(interruptor.isActivated ^ invertInterruptor))
                {
                    isActive = false;
                }
            }
            else
            {
                if(interruptor.isActivated ^ invertInterruptor)
                {
                    isActive = true;
                }
            }
        }
    }

    #region OnValidate

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        speedLerp = Mathf.Max(speedLerp, 0f);
    }

#endif

    #endregion
}
