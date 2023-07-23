using UnityEngine;

[ExecuteAlways]
public class RemoveComponent : MonoBehaviour
{
    [SerializeField] private bool removeComponents = false;
    [SerializeField] private Component componentsTypeToRemove;

    [ExecuteAlways]
    private void Update()
    {
        if(removeComponents)
        {
            RemoveComponents();
            removeComponents = false;
        }
    }

    private void RemoveComponents()
    {
        if(componentsTypeToRemove == null)
            return;

        Component[] components = GetComponents(componentsTypeToRemove.GetType());
        foreach(Component component in components)
        {
            DestroyImmediate(component);
        }
        componentsTypeToRemove = null;
    }
}
