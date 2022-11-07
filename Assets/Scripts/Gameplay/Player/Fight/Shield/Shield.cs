using UnityEngine;

public abstract class Shield : MonoBehaviour
{
    public float currentValue;//0 => shield cass�, 100 compl�tement charg�
    [HideInInspector] public bool isActive { get; protected set; }
    [HideInInspector] public bool wantEnableShield;//true => le joueur veut activ� le bouclier, false sinon.

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        isActive = false;
    }

    protected virtual void Update()
    {

    }

    public virtual bool TryBlockAttack(Attack attack)
    {
        if (!isActive)
            return false;

        if(attack.attackForce > currentValue)
        {
            currentValue = 0f;
            return false;
        }

        if(attack.attackForce > 0.75f)
        {
            //apply stun
        }
        currentValue -= attack.attackForce;
        return true;
    }
}
