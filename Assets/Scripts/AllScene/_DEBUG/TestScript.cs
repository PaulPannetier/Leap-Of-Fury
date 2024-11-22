#if UNITY_EDITOR

using UnityEngine;
using System;

public class TestScript : MonoBehaviour
{
    public bool test;

    private uint factorial(uint n)
    {
        uint res = 1u;
        for (uint i = 1
            ; i <= n; i++)
        {
            res *= i;
        }
        return res;
    }

    private uint factorialIntegral(uint n)
    {
        float GammaIntegrande(float t)
        {
            return Mathf.Pow(t, n) * Mathf.Exp(-t);
        }

        return (uint)Mathf.Round(Useful.Integrate(GammaIntegrande, 0f, 10000f, 0.5f));
    }

    private void OnValidate()
    {
        if(test)
        {
            test = false;
            for (uint i = 0; i < 10; i++)
            {
                print(factorial(i));
                print(factorialIntegral(i));
            }
        }
    }
}

#endif
