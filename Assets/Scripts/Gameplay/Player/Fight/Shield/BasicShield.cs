using UnityEngine;

public class BasicShield : Shield
{
    protected Material shieldMat;
    protected Vector2 shaderOffset = Vector2.zero;
    protected Vector3 maxScale;//le scale du shield quand il est totalement chargé;
    protected bool canBeActivated = true;

    [SerializeField] protected GameObject shieldGO;
    [SerializeField] protected Vector2 shaderSpeed = Vector2.one;
    [Tooltip("Régen du bouclier en %/sec")] [SerializeField] protected float shieldRegen = 15f;
    [Tooltip("Dégénération du bouclier quand il est actif en %age/sec")] [SerializeField] protected float shieldDeregen = 8f;
    [SerializeField] protected Vector3 minShieldScale = new Vector3(0.5f, 0.5f, 1f);

    protected override void Awake()
    {
        base.Awake();
        shieldMat = shieldGO.GetComponent<SpriteRenderer>().material;
    }

    protected override void Start()
    {
        base.Start();
        shieldGO.SetActive(false);
        maxScale = shieldGO.transform.localScale;
        currentValue = 100f;
        canBeActivated = true;
    }

    protected override void Update()
    {
        canBeActivated = currentValue > shieldRegen;

        if (isActive)
        {
            currentValue -= shieldDeregen * Time.deltaTime;
            if(currentValue <= 0f)
            {
                currentValue = 0f;
                isActive = canBeActivated = false;
            }
            shaderOffset += shaderSpeed * Time.deltaTime;
            shieldMat.SetVector("_Offset", shaderOffset);
            shieldGO.transform.localScale = Vector3.Lerp(minShieldScale, maxScale, currentValue / 100f);
        }
        else
        {
            currentValue = Mathf.Min(100f, currentValue + shieldRegen * Time.deltaTime);
        }

        isActive = wantEnableShield && (canBeActivated || isActive);
        shieldGO.SetActive(isActive);
    }

    public override bool TryBlockAttack(Attack attack)
    {
        return base.TryBlockAttack(attack);
    }
}
