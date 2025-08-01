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
                string errorMessage = $"The language : {value} doesn't exist";
                LogManager.instance.AddLog(errorMessage, new object[] { value });
                Debug.LogWarning(errorMessage);
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
                LogManager.instance.AddLog(warningText, new object[] { language, path });
                return;
            }

            dico = new Dictionary<string, string>();
            foreach(LanguageData.ItemData itemData in languageData.data)
            {
                if(dico.ContainsKey(itemData.textID))
                {
                    string warningText = $"The extID : {itemData.textID} already exist in the languagee : {language}";
                    Debug.LogWarning(warningText);
                    LogManager.instance.AddLog(warningText, new object[] { language, itemData });
                    continue;
                }
                dico.Add(itemData.textID, itemData.content);
            }
        }

        defaultLanguageData = new Dictionary<string, string>();
        LoadLanguage(defaultLanguage, @"/Save/GameData/Language/English/text" + SettingsManager.saveFileExtension, ref defaultLanguageData);
        languageData = new Dictionary<string, string>();
        LoadLanguage(currentlanguage, @"/Save/GameData/Language/" + currentlanguage + "/text" + SettingsManager.saveFileExtension, ref languageData);
    }

    public GameText GetText(string textID)
    {
		string content;
        if(languageData.TryGetValue(textID, out content))
            return new GameText(content);

        string errorMsg = $"The text with id : {textID} with the language : {currentlanguage} doesn't exist";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { textID, currentlanguage });

        if (defaultLanguageData.TryGetValue(textID, out content))
            return new GameText(content);

        errorMsg = $"The text with id : {textID} with the default language doesn't exist";
        Debug.LogWarning(errorMsg);
        LogManager.instance.AddLog(errorMsg, new object[] { textID, defaultLanguage });
        return null;
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
