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

    private Dictionary<string, object[]> oldScenesData;
    private Dictionary<string, AsyncOperationHandle<SceneInstance>> preloadSceneAsyncOperationHandle;//addressable

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

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        oldScenesData = new Dictionary<string, object[]>();
        preloadSceneAsyncOperationHandle = new Dictionary<string, AsyncOperationHandle<SceneInstance>>();
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

    public object[] GetOldSceneData(string oldSceneName)
    {
        if (oldScenesData.TryGetValue(oldSceneName, out object[] res))
            return res;
        return null;
    }

    public void SetOldSceneData(object[] oldSceneData)
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (oldScenesData.ContainsKey(activeSceneName))
            oldScenesData[activeSceneName] = oldSceneData;
        else
            oldScenesData.Add(activeSceneName, oldSceneData);
    }

    public bool IsPreloadedSceneComplete(string preloadedSceneName)
    {
        if (preloadSceneAsyncOperationHandle.TryGetValue(preloadedSceneName, out AsyncOperationHandle<SceneInstance> asyncOperation))
            return asyncOperation.IsDone;
        return false;
    }

    public void LoadPreloadedScene(string preloadedSceneName, object[] oldSceneData)
    {
        if (preloadSceneAsyncOperationHandle.TryGetValue(preloadedSceneName, out AsyncOperationHandle<SceneInstance> asyncOperation))
        {
            SetOldSceneData(oldSceneData);
            StartCoroutine(DeletePreloadedUselessScenesThenLauchTheNewScene(preloadedSceneName));
        }
    }

    public void LoadPreloadedScene(string preloadedSceneName)
    {
        if(preloadSceneAsyncOperationHandle.TryGetValue(preloadedSceneName, out AsyncOperationHandle<SceneInstance> asyncOperation))
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
            foreach (string key in preloadSceneAsyncOperationHandle.Keys)
            {
                if (preloadSceneAsyncOperationHandle[key].PercentComplete < 1f)
                    cond = true;
            }
            yield return null;
        }

        AsyncOperationHandle<SceneInstance> tmp = preloadSceneAsyncOperationHandle[preloadedSceneName];
        OnSceneLoad(tmp);
        tmp.Result.ActivateAsync();
    }

    public void LoadScene(string sceneName, object[] oldSceneData = null)
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (oldScenesData.ContainsKey(activeSceneName))
            oldScenesData[activeSceneName] = oldSceneData;
        else
            oldScenesData.Add(activeSceneName, oldSceneData);

        OnSceneLoad();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadSceneAsync(string sceneName, object[] oldSceneData = null)
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (oldScenesData.ContainsKey(activeSceneName))
            oldScenesData[activeSceneName] = oldSceneData;
        else
            oldScenesData.Add(activeSceneName, oldSceneData);

        if (preloadSceneAsyncOperationHandle.TryGetValue(sceneName, out AsyncOperationHandle<SceneInstance> loadSceneAsyncOperationHandle))
        {
            Debug.Log("The scene : " + sceneName + " was already loading in async, loading percentage : " + loadSceneAsyncOperationHandle.PercentComplete);
            return;
        }

        loadSceneAsyncOperationHandle = Addressables.LoadSceneAsync(getSceneAddresseFromSceneName[sceneName], LoadSceneMode.Single, true, 100);
        loadSceneAsyncOperationHandle.Completed += OnSceneLoad;
    }

    private void OnSceneLoad(AsyncOperationHandle<SceneInstance> asyncOperationHandle)
    {
        OnSceneLoad();
    }

    private void OnSceneLoad()
    {
        preloadSceneAsyncOperationHandle.Clear();
        CloneParent.cloneParent.DestroyChildren();
    }

    public void PreLoadScene(string sceneName, object[] oldSceneData)
    {
        if (preloadSceneAsyncOperationHandle.TryGetValue(sceneName, out AsyncOperationHandle<SceneInstance> loadSceneAsyncOperationHandle))
        {
            Debug.Log("The scene : " + sceneName + " was already preloaded, loading percentage : " + loadSceneAsyncOperationHandle.PercentComplete);
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (oldScenesData.ContainsKey(activeSceneName))
            oldScenesData[activeSceneName] = oldSceneData;
        else
            oldScenesData.Add(activeSceneName, oldSceneData);

        loadSceneAsyncOperationHandle = Addressables.LoadSceneAsync(getSceneAddresseFromSceneName[sceneName], LoadSceneMode.Single, false, 100);
        preloadSceneAsyncOperationHandle.Add(sceneName, loadSceneAsyncOperationHandle);
    }
}
