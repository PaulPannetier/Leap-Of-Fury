using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;

    private Dictionary<string, string> languageData;
    private Dictionary<string, string> defaultLanguageData;

    public string defaultLanguage => availableLanguage[0];
    public string[] availableLanguage { get; private set; } = new string[2]
    {
        "English", "Francais"
    };

    private string _currentlanguage;
    public string currentlanguage
    {
        get => _currentlanguage;
        set
        {
            if(availableLanguage.Contains(value))
            {
                if (_currentlanguage != value)
                {
                    _currentlanguage = value;
                    LoadTextLanguage();
                }
            }
            else
            {
                LogManager.instance.AddLog("The language : " + value + " doesn't exist", value);
                Debug.LogWarning("The language : " +  value + " doesn't exist");
            }
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void LoadTextLanguage()
    {
        void LoadLanguage(string language, string path, ref Dictionary<string, string> dico)
        {
            if (!Save.ReadJSONData(path, out LanguageData languageData))
            {
                string warningText = $"Can't load the language : {language} in the path : {path}.";
                Debug.LogWarning(warningText);
                LogManager.instance.AddLog(warningText, language, path);
                return;
            }

            dico = new Dictionary<string, string>();
            foreach(LanguageData.ItemData itemData in languageData.data)
            {
                dico.Add(itemData.textID, itemData.content);
            }
        }

        defaultLanguageData = new Dictionary<string, string>();
        LoadLanguage(defaultLanguage, @"/Save/Language/English/text" + SettingsManager.saveFileExtension, ref defaultLanguageData);
        languageData = new Dictionary<string, string>();
        LoadLanguage(currentlanguage, @"/Save/Language/" + currentlanguage + "/text" + SettingsManager.saveFileExtension, ref languageData);
    }

    private string ApplyGameStatsInText(string content)
    {
        List<Vector2Int> dollarsIndices = new List<Vector2Int>();

        bool firstDollardFound = false;
        int firstDollardIndex = -1;
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i] == '$')
            {
                if(firstDollardFound)
                {
                    dollarsIndices.Add(new Vector2Int(firstDollardIndex, i));
                    firstDollardFound = false;
                    firstDollardIndex = -1;
                }
                else
                {
                    firstDollardFound = true;
                    firstDollardIndex = i;
                }
            }
            else if(content[i] == ' ')
            {
                firstDollardFound = false;
                firstDollardIndex = -1;
            }
        }

        int Comparer(Vector2Int a, Vector2Int b)
        {
            if (a.x == b.x)
                return 0;
            return a.x < b.x ? 1 : -1; 
        }

        dollarsIndices.Sort(Comparer);

        for (int i = 0; i < dollarsIndices.Count; i++)
        {
            Vector2Int indices = dollarsIndices[i];
            string statsID = content.Substring(indices.x + 1, indices.y - indices.x - 1);
            string stat = GameStatisticManager.instance.GetStat(statsID);
            string start = content.Substring(0, indices.x);
            string end = content.Substring(indices.y + 1, content.Length - indices.y - 1);
            content = start + stat + end;
        }

        return content;
    }

    public string GetText(string textID)
    {
        string content;
        if(languageData.TryGetValue(textID, out content))
            return ApplyGameStatsInText(content);
        Debug.LogWarning("The text with id : " + textID + " with the language : " + currentlanguage + " doesn't exist");
        LogManager.instance.AddLog("The text with id : " + textID + " with the language : " + currentlanguage + " doesn't exist");

        if (defaultLanguageData.TryGetValue(textID, out content))
            return ApplyGameStatsInText(content);
        Debug.LogWarning("The text with id : " + textID + " with the default language doesn't exist");
        LogManager.instance.AddLog("The text with id : " + textID + " with the default language doesn't exist");
        return string.Empty;
    }

    [Serializable]
    private struct LanguageData
    {
        public string language;
        public ItemData[] data;

        public LanguageData(ItemData[] data, string language)
        {
            this.data = data;
            this.language = language;
        }

        [Serializable]
        public struct ItemData
        {
            public string textID, content;

            public ItemData(string textID, string content)
            {
                this.textID = textID;
                this.content = content;
            }
        }
    }
}
