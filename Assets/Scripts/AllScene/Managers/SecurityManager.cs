using UnityEngine;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System;

public class SecurityManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private bool saveHash;
#endif

    public bool enableBehaviour;

    [SerializeField] private string[] hashes = { };
    public string[] folderToVerify;

    private void Awake()
    {
        if (!enableBehaviour)
            Destroy(this);
    }

    private void Start()
    {
#if UNITY_EDITOR
        Destroy(this);
        if(Application.isEditor)
        {
            return;
        }
#endif

        if (hashes == null || folderToVerify == null || hashes.Length != folderToVerify.Length)
        {
            LogManager.instance.AddLog("Inconsistent array size, the two array must have the same size.", hashes.Length, folderToVerify.Length, hashes, folderToVerify, "SecurityManager.Start");
            Destroy(this);
            return;
        }

        Hash[] runtimeHashes = ComputeHashes();
        if(hashes.Length != runtimeHashes.Length)
        {
            LogManager.instance.AddLog("Runtime hash and save hash have not the same size.", runtimeHashes.Length, hashes.Length, runtimeHashes, hashes, "SecurityManager.Start");
            Destroy(this);
            return;
        }

        bool error = false;
        for(int i = 0; i < hashes.Length; i++)
        {
            if (hashes[i] != runtimeHashes[i].ToString())
            {
                error = true;
                LogManager.instance.AddLog($"The secure folder {folderToVerify[i]} was modified", folderToVerify[i], hashes[i], runtimeHashes[i], "SecurityManager.Start");
            }
        }

        if(error)
        {
            LogManager.instance.AddLog("File corrupted found, close the application", "SecurityManager.Start");
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }

        Destroy(this);
    }

    public void SaveHashes()
    {
        Hash[] tmpHashes = ComputeHashes();
        hashes = new string[tmpHashes.Length];
        for (int i = 0; i < hashes.Length; i++)
        {
            hashes[i] = tmpHashes[i].ToString();
        }
    }

    private Hash[] ComputeHashes()
    {
        string[] contents = GetFoldersContents();
        Hash[] hashes = new Hash[contents.Length];

        for (int i = 0; i < contents.Length; i++)
        {
            SHA256 shaaaaaw = SHA256.Create();
            hashes[i] = new Hash(shaaaaaw.ComputeHash(StringToByte(contents[i])));
            shaaaaaw.Clear();
            shaaaaaw.Dispose();
        }

        return hashes;
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

    private string[] GetFoldersContents()
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
                    if(file.Contains(".meta"))
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

        Array.Sort(folderToVerify);
        string[] contents = new string[folderToVerify.Length];
        for (int i = 0; i < contents.Length; i++)
        {
            contents[i] = GetFolderString(Path.Combine(Application.dataPath, folderToVerify[i]));
        }

        return contents;
    }

    #region OnValidate

    private void OnValidate()
    {
        Array.Sort(folderToVerify);

        if(saveHash)
        {
            saveHash = false;
            SaveHashes();
        }
    }

    #endregion

    #region struct

    [Serializable]
    private struct Hash
    {
        private readonly byte[] hash;

        public Hash(byte[] hash)
        {
            this.hash = hash;
        }

        public override string ToString() => SecurityManager.ByteToString(hash);

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
