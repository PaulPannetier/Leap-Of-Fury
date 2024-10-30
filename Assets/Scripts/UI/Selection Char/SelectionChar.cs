using UnityEngine;

public class SelectionChar : MonoBehaviour
{
    private void OnEnable()
    {
#if !UNITY_EDITOR
        InputManager.HideMouseCursor();
#endif
    }
}
