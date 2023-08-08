using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    private Dictionary<string, OldSceneData> oldScenesData;
    private string oldSceneName;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        oldScenesData = new Dictionary<string, OldSceneData>();
        oldSceneName = string.Empty;
        preloadSceneAsyncOperation = null;
        preloadSceneAsyncOperationHandleAddressables = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();//addressable
    }

    /*
    private void Update()
    {
        foreach (string preLoadedScene in preLoadedSceneAsyncOperator.Keys)
        {
            preLoadedSceneAsyncOperator[preLoadedScene].priority = preLoadedSceneAsyncOperator[preLoadedScene].progress >= 0.9f ? 0 : 1;
        }
    }
    */

    public OldSceneData GetOldSceneData() => GetOldSceneData(oldSceneName);

    public OldSceneData GetOldSceneData(string sceneName)
    {
        if(oldScenesData.TryGetValue(sceneName, out OldSceneData oldSceneData))
        {
            return oldSceneData;
        }
        return null;
    }

    public void SetOldSceneData(OldSceneData oldSceneData)
    {
        if(this.oldScenesData.ContainsKey(oldSceneData.sceneName))
        {
            this.oldScenesData[oldSceneData.sceneName] = oldSceneData;
        }
        oldScenesData.Add(oldSceneData.sceneName, oldSceneData);
    }

    #region UnitySceneManagement

    private AsyncOperation preloadSceneAsyncOperation;
    private bool isPreloadingAScene;
    private string scenePreload;

    public void LoadScene(string sceneName)
    {
        OnSceneLoad();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadScene(string sceneName, OldSceneData oldSceneData)
    {
        SetOldSceneData(oldSceneData);
        LoadScene(sceneName);
    }

    public void LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncOperation.allowSceneActivation = true;
        asyncOperation.completed += OnSceneLoad;
    }

    public void LoadSceneAsync(string sceneName, OldSceneData oldSceneData)
    {
        SetOldSceneData(oldSceneData);
        LoadSceneAsync(sceneName);
    }

    public void PreloadScene(string sceneName)
    {
        if(isPreloadingAScene)
        {
            Debug.LogWarning("The scene : " + scenePreload + " is already preloaded." + " Can't preload multiples scene with the unity SceneManagement class, use PreloadSceneAddressable instead");
            return;
        }
        isPreloadingAScene = true;
        scenePreload = sceneName;
        preloadSceneAsyncOperation = SceneManager.LoadSceneAsync(sceneName);
        preloadSceneAsyncOperation.allowSceneActivation = false;
        preloadSceneAsyncOperation.completed += OnSceneLoad;
    }

    public void PreloadScene(string sceneName, OldSceneData oldSceneData)
    {
        if (isPreloadingAScene)
        {
            Debug.LogWarning("The scene : " + scenePreload + " is already preloaded." + " Can't preload multiples scene with the unity SceneManagement class, use PreloadSceneAddressable instead");
            return;
        }
        SetOldSceneData(oldSceneData);
        PreloadScene(sceneName);
    }

    public bool IsPreloadedSceneComplete(string sceneName)
    {
        if(!isPreloadingAScene || scenePreload != sceneName)
        {
            Debug.LogWarning("The scene : " + sceneName + " is not preloaded.");
            return false;
        }
        return preloadSceneAsyncOperation.isDone;
    }

    public void LoadPreloadScene(string sceneName)
    {
        if (!isPreloadingAScene || scenePreload != sceneName)
        {
            Debug.LogWarning("The scene : " + sceneName + " is not preloaded.");
            return;
        }
        preloadSceneAsyncOperation.allowSceneActivation = true;
    }

    private void OnSceneLoad(AsyncOperation asyncOperation)
    {
        OnSceneLoad();
    }

    private void OnSceneLoad()
    {
        CloneParent.cloneParent.DestroyChildren();
    }

    #endregion

    #region Addressable

    private Dictionary<string, AsyncOperationHandle<SceneInstance>> preloadSceneAsyncOperationHandleAddressables;//addressable

    public static Dictionary<string, string> getSceneAddresseFromSceneName = new Dictionary<string, string>
    {
        { "Screen Title", "Assets/Scenes/UIs/Screen Title/Screen Title.unity" },
        { "Selection Char", "Assets/Scenes/UIs/Selection Char/Selection Char.unity" },
        { "Selection Map", "Assets/Scenes/UIs/Selection Map/Selection Map.unity" },
        { "Yeti's Cave", "Assets/Scenes/Gameplay/Yeti's Cave/Yeti's Cave.unity" },
        { "Maya's Temple", "Assets/Scenes/Gameplay/Maya's Temple/Maya's Temple.unity" },
        { "Into the Jungle", "Assets/Scenes/Gameplay/Into the Jungle/Into the Jungle.unity" },
        { "Maxwell House", "Assets/Scenes/Gameplay/Maxwell House/Maxwell House.unity" }
    };

    public void LoadSceneAsyncAddressables(string sceneName)
    {
        if (preloadSceneAsyncOperationHandleAddressables.TryGetValue(sceneName, out AsyncOperationHandle<SceneInstance> loadSceneAsyncOperationHandle))
        {
            Debug.Log("The scene : " + sceneName + " was already loading in async, loading percentage : " + loadSceneAsyncOperationHandle.PercentComplete);
            return;
        }

        loadSceneAsyncOperationHandle = Addressables.LoadSceneAsync(getSceneAddresseFromSceneName[sceneName], LoadSceneMode.Single, true, 100);
        loadSceneAsyncOperationHandle.Completed += OnSceneLoadAddressables;
    }

    public void LoadSceneAsyncAddressables(string sceneName, OldSceneData oldSceneData)
    {
        SetOldSceneData(oldSceneData);

        if (preloadSceneAsyncOperationHandleAddressables.TryGetValue(sceneName, out AsyncOperationHandle<SceneInstance> loadSceneAsyncOperationHandle))
        {
            Debug.Log("The scene : " + sceneName + " was already loading in async, loading percentage : " + loadSceneAsyncOperationHandle.PercentComplete);
            return;
        }

        loadSceneAsyncOperationHandle = Addressables.LoadSceneAsync(getSceneAddresseFromSceneName[sceneName], LoadSceneMode.Single, true, 100);
        loadSceneAsyncOperationHandle.Completed += OnSceneLoadAddressables;
    }

    public bool IsPreloadedSceneCompleteAddressables(string preloadedSceneName)
    {
        if (preloadSceneAsyncOperationHandleAddressables.TryGetValue(preloadedSceneName, out AsyncOperationHandle<SceneInstance> asyncOperation))
            return asyncOperation.IsDone;
        return false;
    }

    public void PreLoadSceneAddressables(string sceneName, OldSceneData oldSceneData)
    {
        if (preloadSceneAsyncOperationHandleAddressables.TryGetValue(sceneName, out AsyncOperationHandle<SceneInstance> loadSceneAsyncOperationHandle))
        {
            Debug.Log("The scene : " + sceneName + " was already preloaded, loading percentage : " + loadSceneAsyncOperationHandle.PercentComplete);
            return;
        }

        SetOldSceneData(oldSceneData);

        loadSceneAsyncOperationHandle = Addressables.LoadSceneAsync(getSceneAddresseFromSceneName[sceneName], LoadSceneMode.Single, false, 100);
        preloadSceneAsyncOperationHandleAddressables.Add(sceneName, loadSceneAsyncOperationHandle);
    }

    public void LoadPreloadedSceneAddressables(string preloadedSceneName, OldSceneData oldSceneData)
    {
        if (preloadSceneAsyncOperationHandleAddressables.TryGetValue(preloadedSceneName, out AsyncOperationHandle<SceneInstance> asyncOperation))
        {
            SetOldSceneData(oldSceneData);
            StartCoroutine(DeletePreloadedUselessScenesThenLauchTheNewScene(preloadedSceneName));
        }
    }

    public void LoadPreloadedSceneAddressables(string preloadedSceneName)
    {
        if (preloadSceneAsyncOperationHandleAddressables.TryGetValue(preloadedSceneName, out AsyncOperationHandle<SceneInstance> asyncOperation))
        {
            StartCoroutine(DeletePreloadedUselessScenesThenLauchTheNewScene(preloadedSceneName));
        }
    }

    private IEnumerator DeletePreloadedUselessScenesThenLauchTheNewScene(string preloadedSceneName)
    {
        //on attend que toutes les scene soient bien chargées
        bool cond = true;
        while (cond)
        {
            cond = false;
            foreach (string key in preloadSceneAsyncOperationHandleAddressables.Keys)
            {
                if (preloadSceneAsyncOperationHandleAddressables[key].PercentComplete < 1f)
                    cond = true;
            }
            yield return null;
        }

        AsyncOperationHandle<SceneInstance> tmp = preloadSceneAsyncOperationHandleAddressables[preloadedSceneName];
        OnSceneLoadAddressables(tmp);
        tmp.Result.ActivateAsync();
    }

    private void OnSceneLoadAddressables(AsyncOperationHandle<SceneInstance> asyncOperationHandle)
    {
        OnSceneLoadAddressable();
    }

    private void OnSceneLoadAddressable()
    {
        preloadSceneAsyncOperationHandleAddressables.Clear();
        CloneParent.cloneParent.DestroyChildren();
    }

    #endregion

}

public abstract class OldSceneData
{
    public string sceneName;

    public OldSceneData(string sceneName)
    {
        this.sceneName = sceneName;
    }
}
