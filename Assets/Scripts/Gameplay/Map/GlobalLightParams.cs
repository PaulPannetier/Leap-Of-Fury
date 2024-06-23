#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

public class GlobalLightParams : MonoBehaviour
{
    [SerializeField] private Color globalLightColor = Color.white;
    [SerializeField] private float globalLightIntensity = 1f;

    private void Start()
    {
        LightManager.instance.globalLight.intensity = globalLightIntensity;
        LightManager.instance.globalLight.color = globalLightColor;
    }

    #region OnValidate

#if UNITY_EDITOR

    private void OnValidate()
    {
        globalLightIntensity = Mathf.Max(globalLightIntensity, 0f);

        if (PrefabStageUtility.GetCurrentPrefabStage() == null && LightManager.instance != null)
        {
            LightManager.instance.globalLight.intensity = globalLightIntensity;
            LightManager.instance.globalLight.color = globalLightColor;
        }
    }

#endif

#endregion
}
