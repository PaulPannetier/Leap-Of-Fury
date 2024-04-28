#if UNITY_EDITOR

using UnityEngine;
using System.Collections;

public class TestScript : MonoBehaviour
{
    private void Update()
    {
        if(InputManager.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(TestCorout());
        }
    }

    private IEnumerator TestCorout()
    {
        GetComponent<CharacterController>().Freeze();

        float beg = Time.time;
        while(Time.time - beg < 5f)
        {
            yield return null;
            transform.position += Vector3.up * 5f * Time.deltaTime;
        }

        GetComponent<CharacterController>().UnFreeze();
    }
}

#endif
