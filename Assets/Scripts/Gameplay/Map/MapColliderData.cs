using UnityEngine;

public class MapColliderData : MonoBehaviour
{
    public enum GroundType
    {
        normal,
        ice,
        trampoline,
        convoyerBelt
    }

    public GroundType groundType;
    public bool disableAntiKnockHead = false;

}
