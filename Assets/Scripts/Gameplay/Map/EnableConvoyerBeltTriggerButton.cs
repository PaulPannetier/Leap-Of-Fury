using UnityEngine;

public class EnableConvoyerBeltTriggerButton : MonoBehaviour
{
    [SerializeField] private TriggerButton triggerButton;
    [SerializeField] private ConvoyerBelt[] convoyerToEnable;

    private void Awake()
    {
        if(triggerButton == null)
            triggerButton = GetComponentInChildren<TriggerButton>();
        triggerButton.callbackButtonFunctions += Activate;
    }

    private void Activate(GameObject player, bool enable)
    {
        foreach (ConvoyerBelt convoyer in convoyerToEnable)
        {
            convoyer.enableBehaviour = enable;
        }
    }
}
