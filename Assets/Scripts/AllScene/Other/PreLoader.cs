using UnityEngine;

public class PreLoader : MonoBehaviour
{
    [SerializeField] private string[] sceneToPreload;

    private void Start()
    {
        foreach (string sceneName in sceneToPreload)
        {
            TransitionManager.instance.PreLoadScene(sceneName, null);
        }
    }
}
