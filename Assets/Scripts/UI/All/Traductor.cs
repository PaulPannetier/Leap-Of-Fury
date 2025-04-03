using UnityEngine;
using System;
using TMPro;

public class Traductor : MonoBehaviour
{
    private bool hasStart;
    [SerializeField] private TraductableMessage[] messagesToTranslate;

    private void Start()
    {
        hasStart = true;
        TranslateMessages();
    }

    private void OnEnable()
    {
        if(!hasStart)
            return;

        TranslateMessages();
    }

    public void TranslateMessages()
    {
        foreach (TraductableMessage message in messagesToTranslate)
        {
            string translatedMessage = LanguageManager.instance.GetText(message.textId).Resolve();
            message.text = translatedMessage;
        }
    }

    [Serializable]
    private class TraductableMessage
    {
        public string textId;
        [SerializeField] private TextMeshProUGUI _text;
        public string text
        {
            get => _text.text;
            set => _text.text = value;
        }
    }
}
