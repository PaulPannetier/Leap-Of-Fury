using System;
using System.Linq;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;

    private LanguageData languageData;

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
                _currentlanguage = value;
                if (languageData.language != value)
                {
                    LoadTextLanguage();
                }
            }
            else
            {
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
        if (!Save.ReadJSONData(@"/Save/Language/" + currentlanguage + @"/text" + SettingsManager.saveFileExtension, out languageData))
        {
            LoadDefaultLanguage();
        }
    }

    private void LoadDefaultLanguage()
    {
        Save.ReadJSONData(@"/Save/Language/English/text" + SettingsManager.saveFileExtension, out languageData);
    }

    public string GetText(string textID)
    {
        foreach (LanguageData.ItemData itemData in languageData.data)
        {
            if (itemData.textID == textID)
                return itemData.content;
        }
        Debug.LogWarning("The text with id : " + textID + " with the language : " + currentlanguage + " doesn't exist");
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
