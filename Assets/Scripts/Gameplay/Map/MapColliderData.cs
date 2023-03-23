using UnityEngine;

public class MapColliderData : MonoBehaviour
{
    public GroundType groundType;

    public enum GroundType
    {
        normal,
        ice,
        trampoline,
        convoyerBelt
    }
}
