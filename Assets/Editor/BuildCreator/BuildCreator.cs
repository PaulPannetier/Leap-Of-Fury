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
using System;
using Object = UnityEngine.Object;

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

        if (buildCreatorConfig.performBuild)
        {
            List<string> scenesPath = new List<string>();
            foreach (Object sceneNoCast in buildCreatorConfig.otherSceneTobuild)
            {
                if (sceneNoCast == buildCreatorConfig.firstSceneToPlay)
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
                if (atBegining)
                    scenesPath.Insert(0, scenePath);
                else
                    scenesPath.Add(scenePath);
            }

            string buildDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Build");
            if (Directory.Exists(buildDir))
                Directory.Delete(buildDir, true);
            Directory.CreateDirectory(buildDir);

            bool buildSuccess = PerformBuild(buildDir, scenesPath.ToArray());

            //Copy save content
            if (buildSuccess && buildCreatorConfig.copySaveDirectory)
            {
                string saveDirectory = Path.Combine(buildDir, buildCreatorConfig.gameName + "_Data", "Save");
                FileUtil.CopyFileOrDirectory(Path.Combine(Application.dataPath, "Save"), saveDirectory);
                RmMetaFile(saveDirectory);

                void RmMetaFile(string directory)
                {
                    foreach (string file in Directory.GetFiles(directory))
                    {
                        if (file.EndsWith(".meta"))
                            File.Delete(file);
                    }

                    string[] directories = Directory.GetDirectories(directory);
                    foreach (string dir in directories)
                        RmMetaFile(dir);
                }

                //set default configuration
                if (buildCreatorConfig.setDefaultSettingAndInput)
                {
                    File.WriteAllText(Path.Combine(saveDirectory, "UserSave", "configuration" + saveFileExtension), "");

                    //clear stat file
                    File.WriteAllText(Path.Combine(saveDirectory, "UserSave", "stats" + saveFileExtension), Save.Serialize(new StatisticsData(0f, 0f, 0, 0)));

                    //Write default inputs
                    InputManager.LoadConfiguration(@"//" + Path.Combine("Save", "UserSave", "inputs" + saveFileExtension));
                    InputManager.SetCurrentController(BaseController.KeyboardAndGamepad);
                    string tmpPath = Path.Combine("Save", "GameData", "tmp", "inputs.tmp");
                    if (!InputManager.SaveConfiguration(@"//" + tmpPath))
                        Debug.Log("Couldn't save configuration !!!");
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

            Debug.Log($"Scene {scene.name} done.");
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
    }

    private bool PerformBuild(string buildDir, string[] scenesPath)
    {
        bool PerformWindowsBuild()
        {
            //BuildPlayerOptions buildPlayerOptions = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            Debug.Log(buildPlayerOptions.subtarget);
            buildPlayerOptions.options = buildCreatorConfig.developmentBuild ? BuildOptions.CompressWithLz4HC | BuildOptions.Development : BuildOptions.CompressWithLz4;
            buildPlayerOptions.scenes = scenesPath;
            buildPlayerOptions.locationPathName = Path.Combine(buildDir, buildCreatorConfig.gameName + ".exe");
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
            buildPlayerOptions.extraScriptingDefines = buildCreatorConfig.developmentBuild ? new string[] { "ADVANCE_DEBUG" } : Array.Empty<string>();

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, buildCreatorConfig.useIL2CPPCompilation ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            PlayerSettings.productName = buildCreatorConfig.gameName;
            PlayerSettings.companyName = buildCreatorConfig.companyName;
            PlayerSettings.bundleVersion = buildCreatorConfig.version;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, buildCreatorConfig.managedStrippingLevel);

            Debug.Log("Start building for Windows in " + buildDir);
            //BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build for Windows completed successfully! Size: {summary.totalSize / (1024 * 1024)} MB");
                return true;
            }
            else
            {
                Debug.LogError($"Build for Windows failed: {summary.result}");
                return false;
            }
        }

        bool PerformLinuxBuild()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.options = buildCreatorConfig.developmentBuild ? BuildOptions.CompressWithLz4HC | BuildOptions.Development : BuildOptions.CompressWithLz4;
            buildPlayerOptions.scenes = scenesPath;
            buildPlayerOptions.locationPathName = Path.Combine(buildDir, buildCreatorConfig.gameName);
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
            buildPlayerOptions.extraScriptingDefines = buildCreatorConfig.developmentBuild ? new[] { "ADVANCE_DEBUG" } : Array.Empty<string>();

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, buildCreatorConfig.useIL2CPPCompilation ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            PlayerSettings.SetArchitecture(NamedBuildTarget.Standalone, 1); // 1 for x64 on Linux
            PlayerSettings.productName = buildCreatorConfig.gameName;
            PlayerSettings.companyName = buildCreatorConfig.companyName;
            PlayerSettings.bundleVersion = buildCreatorConfig.version;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Standalone, buildCreatorConfig.managedStrippingLevel);

            Debug.Log("Start building for Linux in " + buildDir);
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build for Linux completed successfully! Size: {summary.totalSize / (1024 * 1024)} MB");
                return true;
            }
            else
            {
                Debug.LogError($"Build for Linux failed: {summary.result}");
                return false;
            }
        }

        bool PerformMacOSBuild()
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

            buildPlayerOptions.options = buildCreatorConfig.developmentBuild ? BuildOptions.CompressWithLz4HC | BuildOptions.Development : BuildOptions.CompressWithLz4;
            buildPlayerOptions.scenes = scenesPath;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;
            buildPlayerOptions.target = BuildTarget.StandaloneOSX;
            buildPlayerOptions.locationPathName = Path.Combine(buildDir, buildCreatorConfig.gameName + ".app");

            NamedBuildTarget namedTarget = NamedBuildTarget.Standalone;
            PlayerSettings.SetScriptingBackend(namedTarget, buildCreatorConfig.useIL2CPPCompilation ? ScriptingImplementation.IL2CPP : ScriptingImplementation.Mono2x);
            int architecture = 1;
            if(buildCreatorConfig.buildPlateform == BuildCreatorConfig.BuildPlateform.MacOSAppleSilicon)
            {
                architecture = 2;
            }
            else if (buildCreatorConfig.buildPlateform == BuildCreatorConfig.BuildPlateform.MacOSUniversal)
            {
                architecture = 3;
            }

            PlayerSettings.SetArchitecture(namedTarget, architecture);
            PlayerSettings.SetManagedStrippingLevel(namedTarget, buildCreatorConfig.managedStrippingLevel);
            buildPlayerOptions.extraScriptingDefines = buildCreatorConfig.developmentBuild ? new string[] { "ADVANCE_DEBUG" } : Array.Empty<string>();

            PlayerSettings.productName = buildCreatorConfig.gameName;
            PlayerSettings.companyName = buildCreatorConfig.companyName;
            PlayerSettings.bundleVersion = buildCreatorConfig.version;

            Debug.Log("Starting build for macOS in " + buildDir);
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build for MacOS completed successfully! Size: {summary.totalSize / (1024 * 1024)} MB");
                return true;
            }
            else
            {
                Debug.LogError($"Build for MacOS failed: {summary.result}");
                return false;
            }
        }

        switch (buildCreatorConfig.buildPlateform)
        {
            case BuildCreatorConfig.BuildPlateform.Windows:
                return PerformWindowsBuild();
            case BuildCreatorConfig.BuildPlateform.Linux:
                return PerformLinuxBuild();
            case BuildCreatorConfig.BuildPlateform.MacOSAppleSilicon:
                return PerformMacOSBuild();
            case BuildCreatorConfig.BuildPlateform.MacOSIntel:
                return PerformMacOSBuild();
            case BuildCreatorConfig.BuildPlateform.MacOSUniversal:
                return PerformMacOSBuild();
            default:
                string errorMessage = $"Unsupported plateform : {buildCreatorConfig.buildPlateform}";
                Debug.LogWarning(errorMessage);
                return false;
        }
    }
}

#endif