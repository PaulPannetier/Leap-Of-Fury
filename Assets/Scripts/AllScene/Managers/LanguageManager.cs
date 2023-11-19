using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager instance;

    private LanguageData languageData;
    private LanguageData defaultLanguageData;

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
        Save.ReadJSONData(@"/Save/Language/English/text" + SettingsManager.saveFileExtension, out defaultLanguageData);
        if (Save.ReadJSONData(@"/Save/Language/" + currentlanguage + @"/text" + SettingsManager.saveFileExtension, out languageData))
        {
            List<LanguageData.ItemData> itemData = new List<LanguageData.ItemData>();

            for (int i = 0; i < defaultLanguageData.data.Length; i++)
            {
                if (!languageData.data.Contains(defaultLanguageData.data[i]))
                {
                    itemData.Add(defaultLanguageData.data[i].Clone());
                }
            }
            defaultLanguageData.data = itemData.ToArray();
        }
        else
        {
            languageData = defaultLanguageData.Clone();
            defaultLanguageData = default(LanguageData);
        }
    }

    public string GetText(string textID)
    {
        foreach (LanguageData.ItemData itemData in languageData.data)
        {
            if (itemData.textID == textID)
                return itemData.content;
        }
        Debug.LogWarning("The text with id : " + textID + " with the language : " + currentlanguage + " doesn't exist");
        if(Application.isPlaying)
        {
            LogManager.instance.WriteLog("The text with id : " + textID + " with the language : " + currentlanguage + " doesn't exist");
        }

        foreach (LanguageData.ItemData itemData in defaultLanguageData.data)
        {
            if (itemData.textID == textID)
                return itemData.content;
        }
        return string.Empty;
    }

    [Serializable]
    private struct LanguageData : ICloneable<LanguageData>
    {
        public string language;
        public ItemData[] data;

        public LanguageData(ItemData[] data, string language)
        {
            this.data = data;
            this.language = language;
        }

        public LanguageData Clone()
        {
            ItemData[] dataClone = new ItemData[data.Length];
            for (int i = 0; i < dataClone.Length; i++)
            {
                dataClone[i] = data[i].Clone();
            }
            return new LanguageData(dataClone, language);
        }

        public override bool Equals(object obj)
        {
            if(!(obj is LanguageData))
                return false;
            LanguageData other = (LanguageData)obj;
            if (other.language != language || other.data.Length != data.Length)
                return false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != other.data[i])
                    return false;
            }
            return true;
        }

        public override int GetHashCode() => HashCode.Combine(data, language);
        public static bool operator ==(LanguageData i1, LanguageData i2) => i1.Equals(i2);
        public static bool operator !=(LanguageData i1, LanguageData i2) => !i1.Equals(i2);

        [Serializable]
        public struct ItemData : ICloneable<ItemData>
        {
            public string textID, content;

            public ItemData(string textID, string content)
            {
                this.textID = textID;
                this.content = content;
            }

            public ItemData Clone() => new ItemData(textID, content);

            public override bool Equals(object other)
            {
                if(!(other is ItemData))
                    return false;

                ItemData obj = (ItemData)other;
                return obj.textID == textID;
            }

            public override int GetHashCode() => HashCode.Combine(textID, content);
            public static bool operator ==(ItemData i1, ItemData i2) => i1.Equals(i2);
            public static bool operator !=(ItemData i1, ItemData i2) => !i1.Equals(i2);
        }
    }
}
