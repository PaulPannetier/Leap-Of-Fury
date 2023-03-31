using System;
using UnityEngine;

public class CharHelpCanvas : MonoBehaviour
{
    private TurningSelector turningSelector;
    private ControllerType controllerType;
    private int id;
    private Action<int> callbackCloseHelpCanvas;

    [SerializeField] private Vector2 turningSelectorOffset;
    [SerializeField] private KeyCode closeButtonKeyboard;
    [SerializeField] private KeyCode closeButtonGamepad1;
    [SerializeField] private KeyCode closeButtonGamepad2;
    [SerializeField] private KeyCode closeButtonGamepad3;
    [SerializeField] private KeyCode closeButtonGamepad4;

    public void Lauch(TurningSelector turningSelector, ControllerType controllerType, int id, Action<int> callbackCloseHelpCanvas)
    {
        this.turningSelector = turningSelector;
        this.controllerType = controllerType;
        transform.position = turningSelector.center + (Vector3)turningSelectorOffset;
        this.id = id;
        this.callbackCloseHelpCanvas = callbackCloseHelpCanvas;
    }

    private void Update()
    {
        if(IsPressingEscape())
        {
            callbackCloseHelpCanvas.Invoke(id);
            Destroy(gameObject);
        }

        bool IsPressingEscape()
        {
            switch (controllerType)
            {
                case ControllerType.Keyboard:
                    return CustomInput.GetKeyDown(KeyCode.Escape) || CustomInput.GetKeyDown(KeyCode.H);
                case ControllerType.Gamepad1:
                    return CustomInput.GetKeyDown(closeButtonGamepad1);
                case ControllerType.Gamepad2:
                    return CustomInput.GetKeyDown(closeButtonGamepad2);
                case ControllerType.Gamepad3:
                    return CustomInput.GetKeyDown(closeButtonGamepad3);
                case ControllerType.Gamepad4:
                    return CustomInput.GetKeyDown(closeButtonGamepad4);
                case ControllerType.GamepadAll:
                    return CustomInput.GetKeyDown(closeButtonGamepad1) || CustomInput.GetKeyDown(closeButtonGamepad2) ||
                        CustomInput.GetKeyDown(closeButtonGamepad3) || CustomInput.GetKeyDown(closeButtonGamepad4);
                case ControllerType.All:
                    return CustomInput.GetKeyDown(KeyCode.Escape) || CustomInput.GetKeyDown(KeyCode.H) || CustomInput.GetKeyDown(closeButtonGamepad1) || CustomInput.GetKeyDown(closeButtonGamepad2) ||
                            CustomInput.GetKeyDown(closeButtonGamepad3) || CustomInput.GetKeyDown(closeButtonGamepad4);
                default:
                    return false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Circle.GizmosDraw((Vector2)transform.position - turningSelectorOffset, 50f);
    }
}
