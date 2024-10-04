#if UNITY_EDITOR

using UnityEngine;
using Collision2D;


public class TestScript : MonoBehaviour
{
    public static Vector3 staticVar = Vector3.zero;

    public const int NUMBER = 5;
    private float primitiveType;
    private Vector2 structVariable;
    private GameObject classVariable;

    private void Method()
    {
        GameObject localVar = classVariable;
        Vector3 localVarStruct = staticVar;
    }
}


public class MyClass
{
    public static Vector3 staticVar = Vector3.zero;

    public const int NUMBER = 5;
    private float primitiveType;
    private Vector2 structVariable;
    private GameObject classVariable;

    private void Method()
    {
        GameObject localVar = classVariable;
        Vector3 localVarStruct = staticVar;
    }
}

#endif
