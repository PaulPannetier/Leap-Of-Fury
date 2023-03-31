using UnityEngine;

public class PreLoader : MonoBehaviour
{
    public bool enableBehaviour = true;
    [SerializeField] private string[] sceneToPreload;

    private void Start()
    {
        if (!enableBehaviour)
            return;
        foreach (string sceneName in sceneToPreload)
        {
            TransitionManager.instance.PreLoadScene(sceneName, null);
        }
    }
}
