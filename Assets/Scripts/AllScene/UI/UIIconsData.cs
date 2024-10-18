using System;
using System.Collections.Generic;
using UnityEngine;

public class UIIconsData : ScriptableObject
{
    public Sprite unknowButton;
    public SerializableDictionary<ControllerModel, InputControllerTypeData> controllerData;

//#if UNITY_EDITOR
//    [SerializeField] private bool generateInputKey = false;
//#endif

    [Serializable]
    public class InputControllerTypeData
    {
        public SerializableDictionary<InputKey, Sprite> buttonsSprite;

        public InputControllerTypeData()
        {
            buttonsSprite = new SerializableDictionary<InputKey, Sprite>();
        }
    }

    #region OnValidate

//#if UNITY_EDITOR

//    private void OnValidate()
//    {
//        if (generateInputKey)
//        {
//            generateInputKey = false;
//            //InputControllerTypeData keyboardData = new InputControllerTypeData();
//            //InputControllerTypeData psVitaData = new InputControllerTypeData();
//            //InputControllerTypeData pS3Data = new InputControllerTypeData();
//            //InputControllerTypeData pS4Data = new InputControllerTypeData();
//            //InputControllerTypeData pS5Data = new InputControllerTypeData();
//            //InputControllerTypeData streamDeckData = new InputControllerTypeData();
//            //InputControllerTypeData switchData = new InputControllerTypeData();
//            //InputControllerTypeData xBox360Data = new InputControllerTypeData();
//            //InputControllerTypeData xBoxOneData = new InputControllerTypeData();
//            //InputControllerTypeData xBoxSeriesData = new InputControllerTypeData();
//            //InputControllerTypeData amazonLunaData = new InputControllerTypeData();
//            //InputControllerTypeData ouyaData = new InputControllerTypeData();

//            //foreach (KeyboardKey kbKey in Enum.GetValues(typeof(KeyboardKey)))
//            //{
//            //    keyboardData.buttonsSprite.Add((InputKey)kbKey, null);
//            //}

//            //foreach (GeneralGamepadKey gamepadKey in Enum.GetValues(typeof(GeneralGamepadKey)))
//            //{
//            //    psVitaData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    pS3Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    pS4Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    pS5Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    streamDeckData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    switchData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    xBox360Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    xBoxOneData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    xBoxSeriesData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    amazonLunaData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //    ouyaData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            //}

//            //List<InputKey> gpkeys = new List<InputKey>();
//            //foreach(GeneralGamepadKey gpKey in Enum.GetValues(typeof(GeneralGamepadKey)))
//            //{
//            //    gpkeys.Add((InputKey)gpKey);
//            //}

//            //for (int i = 0; i < controllerData.elements.Count; i++)
//            //{
//            //    if (controllerData.elements[i].key != ControllerModel.Keyboard)
//            //    {
//            //        for (int j = 0; j < gpkeys.Count; j++)
//            //        {
//            //            controllerData.elements[i].value.buttonsSprite.elements[j] = new SerializableDictionary<InputKey, Sprite>.DictionaryElement(gpkeys[j], controllerData.elements[i].value.buttonsSprite.elements[j].value);
//            //        }
//            //    }
//            //}

//            //controllerData = new SerializableDictionary<ControllerModel, InputControllerTypeData>();
//            //controllerData.Add(ControllerModel.Keyboard, keyboardData);
//            //controllerData.Add(ControllerModel.PSVita, psVitaData);
//            //controllerData.Add(ControllerModel.PS3, pS3Data);
//            //controllerData.Add(ControllerModel.PS4, pS4Data);
//            //controllerData.Add(ControllerModel.PS5, pS5Data);
//            //controllerData.Add(ControllerModel.SteamDeck, streamDeckData);
//            //controllerData.Add(ControllerModel.Switch, switchData);
//            //controllerData.Add(ControllerModel.XBox360, xBox360Data);
//            //controllerData.Add(ControllerModel.XBoxOne, xBoxOneData);
//            //controllerData.Add(ControllerModel.XBoxSeries, xBoxSeriesData);
//            //controllerData.Add(ControllerModel.AmazonLuna, amazonLunaData);
//            //controllerData.Add(ControllerModel.Ouya, ouyaData);
//        }
//    }

//#endif

    #endregion
}
