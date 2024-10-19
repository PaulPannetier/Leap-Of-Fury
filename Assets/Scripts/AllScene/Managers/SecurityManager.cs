using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

public static class SecurityManager
{
    public static void WriteBuildHashed(string buildSavePath)
    {
        Hash hash = ComputeHash();
        string hashJSON = Save.Serialize(hash);
        File.WriteAllText(Path.Combine(buildSavePath, "GameData", "BuildHash" + SettingsManager.saveFileExtension), hashJSON);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void Start()
    {

#if UNITY_EDITOR

        if(Application.isEditor)
            return;

#endif

        Hash runtimeHash = ComputeHash();
        string buildHashFile = $"/Save/GameData/BuildHash" + SettingsManager.saveFileExtension;
        if (!Save.ReadJSONData<Hash>(buildHashFile, out Hash buildHash))
        {
            string errorMsg = $"Can't load build hashes file : {buildHashFile}.";
            Debug.LogWarning(errorMsg);
            QuitGame();
            return;
        }

        if (buildHash != runtimeHash)
        {
            QuitGame();
        }
    }

    private static void QuitGame()
    {
#if !UNITY_EDITOR

            Application.Quit();

#endif
    }

    private static Hash ComputeHash()
    {
        string contents = GetGameDataContents();
        SHA256 shaaaaaw = SHA256.Create();
        byte[] hashByte = shaaaaaw.ComputeHash(StringToByte(contents));
        Hash hash = new Hash(ByteToString(hashByte));
        shaaaaaw.Clear();
        shaaaaaw.Dispose();
        return hash;
    }

    private static byte[] StringToByte(string content) => Encoding.ASCII.GetBytes(content);
    private static string ByteToString(byte[] array)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            sb.Append($"{array[i]:X2}");
        }
        return sb.ToString();
    }

    private static string GetGameDataContents()
    {
        string GetFolderString(string dir)
        {
            void GetFoldersStringRecur(string dir, ref StringBuilder sb)
            {
                string[] files = Directory.GetFiles(dir);
                Array.Sort(files);
                string[] dirs = Directory.GetDirectories(dir);
                Array.Sort(dirs);

                foreach (string file in files)
                {
                    if(file.Contains(".meta") || file.EndsWith("BuildHash" + SettingsManager.saveFileExtension))
                        continue;
                    string content = File.ReadAllText(Path.Combine(dir, file));
                    sb.Append(content);
                }

                foreach (string directory in dirs)
                {
                    GetFoldersStringRecur(directory, ref sb);
                }
            }

            StringBuilder sb = new StringBuilder();
            GetFoldersStringRecur(dir, ref sb);

            return sb.ToString();
        }

        return GetFolderString(Path.Combine(Application.dataPath, "Save", "GameData"));
    }

    #region Struct

    [Serializable]
    private struct Hash
    {
        [SerializeField] private string hash;

        public Hash(string hash)
        {
            this.hash = hash;
        }

        public override string ToString() => hash;

        public override int GetHashCode() => hash.GetHashCode();

        public override bool Equals(object obj)
        {
            if(object.ReferenceEquals(obj, null) && object.ReferenceEquals(hash, null))
            {
                return true;
            }
            if (object.ReferenceEquals(obj, null) || object.ReferenceEquals(hash, null))
            {
                return false;
            }

            if(obj is Hash hash2)
            {
                return hash2.hash == this.hash;
            }

            return false;
        }

        public static bool operator ==(Hash h1, Hash h2) => h1.hash == h2.hash;
        public static bool operator!=(Hash h1, Hash h2) => !(h1 == h2);
    }

    #endregion
}
