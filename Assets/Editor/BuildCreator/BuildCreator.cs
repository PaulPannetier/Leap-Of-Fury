#if UNITY_EDITOR

using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using static SettingsManager;
using static StatisticsManager;

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
            foreach (Object sceneNoCast in buildCreatorConfig.otherSceneTobuild)
            {
                if(sceneNoCast == buildCreatorConfig.firstSceneToPlay)
                {
                    continue;
                }
                AddToScenesPath(scenesPath, sceneNoCast);
            }
            foreach (Object sceneNoCast in buildCreatorConfig.gameplayScenes)
            {
                if (sceneNoCast == buildCreatorConfig.firstSceneToPlay)
                {
                    continue;
                }
                AddToScenesPath(scenesPath, sceneNoCast);
            }

            AddToScenesPath(scenesPath, buildCreatorConfig.firstSceneToPlay, true);

            void AddToScenesPath(List<string> scenesPath, Object sceneNoCast, bool atBegining = false)
            {
                SceneAsset sceneAsset = sceneNoCast as SceneAsset;
                if (sceneAsset == null)
                    return;
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if(atBegining)
                {
                    scenesPath.Insert(0, scenePath);
                }
                else
                {
                    scenesPath.Add(scenePath);
                }
            }

            string buildDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Build");
            if(Directory.Exists(buildDir))
                Directory.Delete(buildDir, true);
            Directory.CreateDirectory(buildDir);

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.options = buildCreatorConfig.developpementBuild ? BuildOptions.CompressWithLz4HC | BuildOptions.Development : BuildOptions.CompressWithLz4;
            buildPlayerOptions.scenes = scenesPath.ToArray();
            buildPlayerOptions.locationPathName = Path.Combine(buildDir, "PartyGame.exe");
            buildPlayerOptions.target = BuildTarget.StandaloneWindows;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
            buildPlayerOptions.extraScriptingDefines = buildCreatorConfig.advancedDebug ? new string[] { "ADVANCE_DEBUG" } : new string[] { };
            Debug.Log("Start building at " + buildDir);
            //BuildReport reporting = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);

            //Copy save content
            if(buildCreatorConfig.copySaveDirectory)
            {
                string saveDirectory = Path.Combine(buildDir, "PartyGame_Data", "Save");
                FileUtil.CopyFileOrDirectory(Path.Combine(Application.dataPath, "Save"), saveDirectory);
                RmMetaFile(Path.Combine(saveDirectory));

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
                if(buildCreatorConfig.setDefaultSettingAndInput)
                {
                    File.WriteAllText(Path.Combine(saveDirectory, "configuration" + saveFileExtension),  "");

                    //clear stat file
                    File.WriteAllText(Path.Combine(saveDirectory, "stats" + saveFileExtension), Save.ConvertObjectToJSONString(new StatisticsData(0f, 0f, 0, 0)));

                    InputManager.LoadConfiguration(@"//" + Path.Combine("Save", "inputs" + saveFileExtension));
                    InputManager.SetCurrentController(BaseController.KeyboardAndGamepad);
                    InputManager.SaveConfiguration(@"//" + Path.Combine("Save", "tmp", "inputs.tmp"));
                    string tmpPath = Path.Combine(Application.dataPath, "Save", "tmp", "inputs.tmp");
                    string inputsText = File.ReadAllText(tmpPath);
                    File.WriteAllText(Path.Combine(saveDirectory, "inputs" + saveFileExtension), inputsText);
                    File.Delete(tmpPath);
                    InputManager.LoadConfiguration(@"//" + Path.Combine("Save", "inputs" + saveFileExtension));
                }

                //reset log file
                File.WriteAllText(Path.Combine(saveDirectory, "Log.txt"), "");

                //clear tmp folder
                Directory.Delete(Path.Combine(saveDirectory, "tmp"), true);
                Directory.CreateDirectory(Path.Combine(saveDirectory, "tmp"));
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

#endif