using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour
{
    public static Light2D globalLight {  get; private set; }

    private void Awake()
    {
        globalLight = GetComponent<Light2D>();
    }
}
