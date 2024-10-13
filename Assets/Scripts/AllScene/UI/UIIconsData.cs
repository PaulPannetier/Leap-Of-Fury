using System;
using UnityEngine;

public class UIIconsData : ScriptableObject
{
    public Sprite unknowButton;
    public SerializableDictionary<InputControllerType, InputControllerTypeData> controllerData;

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
//        if(generateInputKey)
//        {
//            generateInputKey = false;
//            InputControllerTypeData keyboardData = new InputControllerTypeData();
//            InputControllerTypeData psVitaData = new InputControllerTypeData();
//            InputControllerTypeData pS3Data = new InputControllerTypeData();
//            InputControllerTypeData pS4Data = new InputControllerTypeData();
//            InputControllerTypeData pS5Data = new InputControllerTypeData();
//            InputControllerTypeData streamDeckData = new InputControllerTypeData();
//            InputControllerTypeData switchData = new InputControllerTypeData();
//            InputControllerTypeData xBox360Data = new InputControllerTypeData();
//            InputControllerTypeData xBoxOneData = new InputControllerTypeData();
//            InputControllerTypeData xBoxSeriesData = new InputControllerTypeData();
//            InputControllerTypeData amazonLunaData = new InputControllerTypeData();
//            InputControllerTypeData ouyaData = new InputControllerTypeData();

//            foreach (KeyboardKey kbKey in Enum.GetValues(typeof(KeyboardKey)))
//            {
//                keyboardData.buttonsSprite.Add((InputKey)kbKey, null);
//            }

//            foreach (GeneralGamepadKey gamepadKey in Enum.GetValues(typeof(GeneralGamepadKey)))
//            {
//                psVitaData.buttonsSprite.Add((InputKey)gamepadKey, null);
//                pS3Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//                pS4Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//                pS5Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//                streamDeckData.buttonsSprite.Add((InputKey)gamepadKey, null);
//                switchData.buttonsSprite.Add((InputKey)gamepadKey, null);
//                xBox360Data.buttonsSprite.Add((InputKey)gamepadKey, null);
//                xBoxOneData.buttonsSprite.Add((InputKey)gamepadKey, null);
//                xBoxSeriesData.buttonsSprite.Add((InputKey)gamepadKey, null);
//                amazonLunaData.buttonsSprite.Add((InputKey)gamepadKey, null);
//                ouyaData.buttonsSprite.Add((InputKey)gamepadKey, null);
//            }

//            controllerData = new SerializableDictionary<InputControllerType, InputControllerTypeData>();
//            controllerData.Add(InputControllerType.Keyboard, keyboardData);
//            controllerData.Add(InputControllerType.PSVita, psVitaData);
//            controllerData.Add(InputControllerType.PS3, pS3Data);
//            controllerData.Add(InputControllerType.PS4, pS4Data);
//            controllerData.Add(InputControllerType.PS5, pS5Data);
//            controllerData.Add(InputControllerType.SteamDeck, streamDeckData);
//            controllerData.Add(InputControllerType.Switch, switchData);
//            controllerData.Add(InputControllerType.XBox360, xBox360Data);
//            controllerData.Add(InputControllerType.XBoxOne, xBoxOneData);
//            controllerData.Add(InputControllerType.XBoxSeries, xBoxSeriesData);
//            controllerData.Add(InputControllerType.AmazonLuna, amazonLunaData);
//            controllerData.Add(InputControllerType.Ouya, ouyaData);
//        }
//    }

//#endif

    #endregion
}
