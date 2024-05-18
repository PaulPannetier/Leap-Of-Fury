using UnityEngine;

public abstract class ActivableObject : MonoBehaviour
{
    [SerializeField] protected bool startActivated = true;

    public bool isActivated {  get; protected set; }

    protected virtual void Start()
    {
        isActivated = startActivated;
        if (startActivated)
            OnActivated();
        else
            OnDesactivated();
    }

    protected abstract void OnActivated();
    protected abstract void OnDesactivated();

}
