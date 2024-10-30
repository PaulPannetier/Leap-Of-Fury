using UnityEngine;

public class SelectionMap : MonoBehaviour
{
    private void OnEnable()
    {
#if !UNITY_EDITOR
        InputManager.HideMouseCursor();
#endif
    }
}
