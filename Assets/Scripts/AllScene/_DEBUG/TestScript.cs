#if UNITY_EDITOR

using UnityEngine;
using System;

public class TestScript : MonoBehaviour
{
    [SerializeField] private bool test;

    private void Test()
    {
        Polynome p0 = new Polynome(new float[] { 0.5f, -4, 2, 1 });
        Polynome p1 = new Polynome(new float[] { 1f, -4, 2, -3 });

        SubTest(p0);
        SubTest(p1);

        void SubTest(Polynome p)
        {
            print(p);
            float[] roots = p.Roots();
            foreach (float root in roots)
            {
                print(root + " accuracy : " + p.Evaluate(root));
            }
        }
    }

    private void OnValidate()
    {
        if(test)
        {
            test = false;
            Test();
        }
    }
}

#endif
