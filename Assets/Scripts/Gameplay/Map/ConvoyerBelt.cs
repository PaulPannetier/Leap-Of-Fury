using UnityEngine;

[RequireComponent(typeof(MapColliderData))]
public class ConvoyerBelt : MonoBehaviour
{
    private MapColliderData mapColliderData;

    private bool _isActive;
    private bool isActive
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

    public float maxSpeed;
    public float speedLerp;

    [Header("Use by Interruptor")]
    [SerializeField] private bool useByInterruptor;
    [SerializeField] private bool invertInterruptor;
    [SerializeField] private Interruptor interruptor;

    private void Awake()
    {
        mapColliderData = GetComponent<MapColliderData>();
        if(useByInterruptor)
            isActive = false;
    }

    private void Update()
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

    private void OnValidate()
    {
        speedLerp = Mathf.Max(speedLerp, 0f);
    }

#endif

    #endregion
}
