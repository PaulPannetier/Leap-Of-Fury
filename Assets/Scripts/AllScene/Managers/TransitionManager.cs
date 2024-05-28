using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager instance;

    private Dictionary<string, OldSceneData> oldScenesData;
    private AsyncOperation preloadSceneAsyncOperation;
    private bool isPreloadingAScene;
    private string scenePreload;

    public string oldSceneName { get; private set; }
    public string activeScene => SceneManager.GetActiveScene().name;

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
    }

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
        if (oldSceneData == null)
            return;

        if(oldScenesData.ContainsKey(oldSceneData.sceneName))
        {
            oldScenesData[oldSceneData.sceneName] = oldSceneData;
        }
        else
        {
            oldScenesData.Add(oldSceneData.sceneName, oldSceneData);
        }

        oldSceneName = oldSceneData.sceneName;
    }

    #region UnitySceneManagement

    public void LoadScene(string sceneName)
    {
        OnSceneLoad();
        oldSceneName = sceneName;
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
            string errorText = "Can't prelaod the scene" + sceneName + "because the scene : " + scenePreload + " is already preloaded. Unity SceneManagement class can't also preload multiple scenes.";
            Debug.LogWarning(errorText);
            LogManager.instance.AddLog(errorText, sceneName, scenePreload);
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
            string errorText = "Can't prelaod the scene" + sceneName + "because the scene : " + scenePreload + " is already preloaded. Unity SceneManagement class can't also preload multiple scenes.";
            Debug.LogWarning(errorText);
            LogManager.instance.AddLog(errorText, sceneName, scenePreload);
            return;
        }
        SetOldSceneData(oldSceneData);
        PreloadScene(sceneName);
    }

    public bool IsPreloadedSceneComplete(string sceneName)
    {
        if(!isPreloadingAScene || scenePreload != sceneName)
        {
            string errorMessage = "The scene : " + sceneName + " is not preloaded.";
            Debug.LogWarning(errorMessage);
            LogManager.instance.AddLog(errorMessage, sceneName, scenePreload);
            return false;
        }
        return preloadSceneAsyncOperation.isDone;
    }

    public void LoadPreloadScene(string sceneName)
    {
        if (!isPreloadingAScene || scenePreload != sceneName)
        {
            string errorMessage = "The scene : " + sceneName + " is not preloaded.";
            Debug.LogWarning(errorMessage);
            LogManager.instance.AddLog(errorMessage, sceneName, scenePreload);
            return;
        }
        isPreloadingAScene = false;
        scenePreload = string.Empty;
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
}

public class OldSceneData
{
    public string sceneName;

    public OldSceneData(string sceneName)
    {
        this.sceneName = sceneName;
    }
}
