
using UnityEngine;

public class MainCanvasManager : MonoBehaviour
{
    public static GameObject mainCanvas;

    private void Awake()
    {
        mainCanvas = gameObject;    
    }
}
