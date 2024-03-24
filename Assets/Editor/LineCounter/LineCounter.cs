#if UNITY_EDITOR

using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LineCounterConfig))]
public class LineCounter : Editor
{
    private static LineCounterConfig lineCounterConfig;

    [InitializeOnLoadMethod]
    private static void OnInitialize()
    {
        FetchConfig();
    }

    private static void FetchConfig()
    {
        while (true)
        {
            if (lineCounterConfig != null)
                return;

            string path = GetConfigPath();

            if (path == null)
            {
                AssetDatabase.CreateAsset(CreateInstance<LineCounterConfig>(), $"Assets/{nameof(LineCounterConfig)}.asset");
                Debug.Log("A config file has been created at the root of your project.<b> You can move this anywhere you'd like.</b>");
                continue;
            }

            lineCounterConfig = AssetDatabase.LoadAssetAtPath<LineCounterConfig>(path);

            break;
        }
    }

    private static string GetConfigPath()
    {
        List<string> paths = AssetDatabase.FindAssets(nameof(LineCounterConfig)).Select(AssetDatabase.GUIDToAssetPath).Where(c => c.EndsWith(".asset")).ToList();
        if (paths.Count > 1)
            Debug.LogWarning("Multiple Line Counter config assets found. Delete one.");
        return paths.FirstOrDefault();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("You can move this asset where ever you'd like :p, good code;", MessageType.Info);

        if (GUILayout.Button("Count lines of code!"))
        {
            CountLineOfCodes();
        }
    }

    private void CountLineOfCodes()
    {
        int nbLines = 0;
        int nbFiles = 0;
        foreach (string p in lineCounterConfig.subfolderToCount)
        {
            string path = Application.dataPath + "/" + p;
            if(Directory.Exists(path))
            {
                CountLineRecur(path, ref nbLines, ref nbFiles, lineCounterConfig.fileExtensionsAccepted);
            }
        }
        if(nbLines > 0)
            Debug.Log($"There are {nbFiles} files and {nbLines} lines of code in your directories!");
    }

    private void CountLineRecur(string path, ref int nbLines, ref int nbFiles, List<string> extensions)
    {
        string[] directories = Directory.GetDirectories(path);
        foreach (string directory in directories)
        {
            CountLineRecur(directory, ref nbLines, ref nbFiles, extensions);
            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                bool endWith = false;
                foreach (string ext in extensions)
                {
                    if(file.EndsWith(ext))
                    {
                        endWith = true;
                        break;
                    }
                }
                if(endWith)
                {
                    nbLines += File.ReadAllLines(file).Length;
                    nbFiles++;
                }
            }
        }
    }
}

#endif