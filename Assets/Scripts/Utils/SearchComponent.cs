#if UNITY_EDITOR

using System;
using UnityEngine;

public class SearchComponent : MonoBehaviour
{
    [SerializeField] bool search;
    [SerializeField] private Component componentTypeToSearch;

    [SerializeField] private Component[] componentFound;

    private void OnValidate()
    {
        if(search)
        {
            search = false;

            componentFound = (Component[])GameObject.FindObjectsByType(componentTypeToSearch.GetType(), FindObjectsSortMode.None);
        }
    }
}

#endif
