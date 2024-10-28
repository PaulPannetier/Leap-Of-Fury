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
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEditor.Build;

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

        if (GUILayout.Button("Create build!"))
        {
            CreateBuild();
        }
    }

    private void CreateBuild()
    {
        if (buildCreatorConfig.modifyGameplayScene)
        {
            ModifyGameplayScenes();
        }

        ModifyScreenTitleScene();

        if (buildCreatorConfig.performBuild)
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
            buildPlayerOptions.locationPathName = Path.Combine(buildDir, buildCreatorConfig.gameName + ".exe");
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
            buildPlayerOptions.extraScriptingDefines = buildCreatorConfig.developpementBuild ? new string[] { "ADVANCE_DEBUG" } : new string[] { };

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, buildCreatorConfig.useIL2CPPCompilation ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, (int)buildCreatorConfig.architectureCPU);
            PlayerSettings.productName = buildCreatorConfig.gameName;
            PlayerSettings.companyName = buildCreatorConfig.compagnyName;
            PlayerSettings.bundleVersion = buildCreatorConfig.version;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, buildCreatorConfig.managedStrippingLevel);

            Debug.Log("Start building in " + buildDir);
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);

            //Copy save content
            if(buildCreatorConfig.copySaveDirectory)
            {
                string saveDirectory = Path.Combine(buildDir, buildCreatorConfig.gameName + "_Data", "Save");
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
                    File.WriteAllText(Path.Combine(saveDirectory, "UserSave", "configuration" + saveFileExtension),  "");

                    //clear stat file
                    File.WriteAllText(Path.Combine(saveDirectory, "UserSave", "stats" + saveFileExtension), Save.Serialize(new StatisticsData(0f, 0f, 0, 0)));

                    //Write default inputs
                    InputManager.LoadConfiguration(@"//" + Path.Combine("Save", "UserSave", "inputs" + saveFileExtension));
                    InputManager.SetCurrentController(BaseController.KeyboardAndGamepad);
                    string tmpPath = Path.Combine("Save", "GameData", "tmp", "inputs.tmp");
                    if (!InputManager.SaveConfiguration(@"//" + tmpPath))
						Debug.Log("couldn't save configuration !!!");
                    string inputsText = File.ReadAllText(Path.Combine(Application.dataPath, tmpPath));
                    File.WriteAllText(Path.Combine(saveDirectory, "UserSave", "inputs" + saveFileExtension), inputsText);
                    File.Delete(Path.Combine(Application.dataPath, tmpPath));
                    InputManager.LoadConfiguration(@"//" + Path.Combine("Save", "UserSave", "inputs" + saveFileExtension));
                }

                //reset log file
                File.WriteAllText(Path.Combine(saveDirectory, "UserSave", "Log.txt"), "");

                //clear tmp folder
                Directory.Delete(Path.Combine(saveDirectory, "GameData", "tmp"), true);
                Directory.CreateDirectory(Path.Combine(saveDirectory, "GameData", "tmp"));

                //Write version
                VersionManager.WriteBuildVersion(buildCreatorConfig.version, saveDirectory);

                //Security Manager
                SecurityManager.WriteBuildHashed(saveDirectory);
            }
        }
    }

    private void ModifyScreenTitleScene()
    {
        string currentScenePath = SceneManager.GetActiveScene().path;
        SceneAsset screenTitleAsset = null;
        foreach (Object scene in buildCreatorConfig.otherSceneTobuild)
        {
            if(scene == null || scene.name != "Screen Title")
                continue;
            screenTitleAsset = scene as SceneAsset;
            break;
        }

        if (screenTitleAsset == null)
            return;

        string screenTitlePath = AssetDatabase.GetAssetPath(screenTitleAsset);
        Scene screenTitle = EditorSceneManager.OpenScene(screenTitlePath, OpenSceneMode.Single);

        GameObject gamemanager = null;
        foreach (GameObject go in screenTitle.GetRootGameObjects())
        {
            if (go.name == "Singleton")
            {
                foreach (Transform t in go.transform)
                {
                    if (t.name == "GameManager")
                    {
                        gamemanager = t.gameObject;
                        break;
                    }
                }
            }
        }

        EditorSceneManager.SaveScene(screenTitle, screenTitlePath);
        EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
    }

    private void ModifyGameplayScenes()
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