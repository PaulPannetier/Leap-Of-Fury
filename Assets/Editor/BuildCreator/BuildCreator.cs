using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using static SettingsManager;

[CustomEditor(typeof(BuildCreatorConfig))]
public class BuildCreator : Editor
{
    private static BuildCreatorConfig buildCreatorConfig;

    [InitializeOnLoadMethod]
    private static void OnInitialize()
    {
        FetchConfig();
    }

    private static void FetchConfig()
    {
        while (true)
        {
            if (buildCreatorConfig != null)
                return;

            string path = GetConfigPath();

            if (path == null)
            {
                AssetDatabase.CreateAsset(CreateInstance<BuildCreatorConfig>(), $"Assets/{nameof(BuildCreatorConfig)}.asset");
                Debug.Log("A config file has been created at the root of your project.<b> You can move this anywhere you'd like.</b>");
                continue;
            }

            buildCreatorConfig = AssetDatabase.LoadAssetAtPath<BuildCreatorConfig>(path);
            break;
        }
    }

    private static string GetConfigPath()
    {
        List<string> paths = AssetDatabase.FindAssets(nameof(BuildCreatorConfig)).Select(AssetDatabase.GUIDToAssetPath).Where(c => c.EndsWith(".asset")).ToList();
        if (paths.Count > 1)
            Debug.LogWarning("Multiple Line Counter config assets found. Delete one.");
        return paths.FirstOrDefault();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("You can move this asset where ever you'd like :p, good code;", MessageType.Info);

        if (GUILayout.Button("Create build!"))
        {
            CreateBuild();
        }
    }

    private void CreateBuild()
    {
        if(buildCreatorConfig.modifyGameplayScene)
        {
            ModidyGameplayScenes();
        }

        if(buildCreatorConfig.performBuild)
        {
            List<string> scenesPath = new List<string>();
            foreach (Object sceneNoCast in buildCreatorConfig.gameplayScenes)
            {
                AddToScenesPath(scenesPath, sceneNoCast);
            }
            foreach (Object sceneNoCast in buildCreatorConfig.otherSceneTobuild)
            {
                AddToScenesPath(scenesPath, sceneNoCast);
            }

            void AddToScenesPath(List<string> scenesPath, Object sceneNoCast)
            {
                SceneAsset sceneAsset = sceneNoCast as SceneAsset;
                if (sceneAsset == null)
                    return;
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                scenesPath.Add(scenePath);
            }

            string buildDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Build");
            Directory.Delete(buildDir, true);
            Directory.CreateDirectory(buildDir);

            Debug.Log("Start building at " + buildDir);
            BuildReport reporting = BuildPipeline.BuildPlayer(scenesPath.ToArray(), buildDir + "PartyGame.exe", BuildTarget.StandaloneWindows, BuildOptions.None);

            //Copy save content
            if(buildCreatorConfig.copySaveDirectory)
            {
                Debug.Log("Copy Save directory");

                FileUtil.CopyFileOrDirectory(Path.Combine(Application.dataPath, "Save"), buildDir);

                //Save.Copy(Path.Combine(Application.dataPath, "Save"), buildDir);
                RmMetaFile(Path.Combine(buildDir, "Save"));

                Debug.Log("Save directory fully copied!");

                void RmMetaFile(string directory)
                {
                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".meta"))
                        {
                            File.Delete(file);
                        }
                    }

                    string[] directories = Directory.GetDirectories(directory);
                    foreach (string dir in directories)
                    {
                        RmMetaFile(dir);
                    }
                }

                //set default configuration
                ConfigurationData defaultConfig = new ConfigurationData(new Vector2Int(1920, 1080), new RefreshRate { numerator = 60, denominator = 1 }, "English", FullScreenMode.FullScreenWindow, true);
                Save.WriteJSONData(defaultConfig, @"/Save/configuration" + saveFileExtension);
            }
        }
    }

    private void ModidyGameplayScenes()
    {
        string currentScenePath = SceneManager.GetActiveScene().path;
        foreach (Object sceneNoCast in buildCreatorConfig.gameplayScenes)
        {
            //open the scene
            SceneAsset sceneAsset = sceneNoCast as SceneAsset;
            if (sceneAsset == null)
                continue;
            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            //first remove map tag GO and destroy CharParent's childdren
            GameObject[] rootGO = scene.GetRootGameObjects();
            GameObject singletonGO = null;
            foreach (GameObject go in rootGO)
            {
                if (go.CompareTag("Map"))
                {
                    DestroyImmediate(go);
                    continue;
                }

                if (go.CompareTag("CharsParent"))
                {
                    foreach (Transform t in go.transform)
                    {
                        DestroyImmediate(t.gameObject);
                    }
                    continue;
                }

                if (go.name == "Singleton")
                {
                    singletonGO = go;
                    continue;
                }
            }

            if (singletonGO != null)
            {
                foreach (Transform t in singletonGO.transform)
                {
                    if (t.name == "LevelManager")
                    {
                        t.gameObject.GetComponent<StartLevelManager>().enableBehaviour = true;
                        t.gameObject.GetComponent<LevelManager>().enableBehaviour = true;
                        break;
                    }
                }
            }

            Debug.Log($"Scene {scene.name} done.");
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
    }
}
