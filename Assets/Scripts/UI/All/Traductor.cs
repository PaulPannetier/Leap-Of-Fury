using UnityEngine;
using System;
using TMPro;

public class Traductor : MonoBehaviour
{
    [SerializeField] private TraductableMessage[] messagesToTranslate;

    private void Start()
    {
        TranslateMessages();
    }

    public void TranslateMessages()
    {
        foreach (TraductableMessage message in messagesToTranslate)
        {
            string translatedMessage = LanguageManager.instance.GetText(message.textId);
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
