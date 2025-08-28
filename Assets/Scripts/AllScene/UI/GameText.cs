using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameText
{
    private const string SPRITESHEET = "keys";

	private string[] inputIconsKeywords = new string[] 
	{
        "Key_Back", "Key_Validate", "Key_Left", "Key_Right", "Key_Info"
    };

    private string text;
	private ControllerInfo? modelInfo;

	public GameText(string text)
	{
		this.text = text;
		this.modelInfo = null;
	}

	public string Resolve()
	{
		const string statPattern = @"\$stat=([\w\d]+?)\$"; // Ex: "SuperSpell has an impedance of only $stat=superspell_impedance$ !"
		const string spritePattern = @"\$sprite=([\w\d]+?)\$"; // Ex: "Press $sprite=Key_Esc_Dark$ to pause"

		string res = text;
		res = Regex.Replace(res, statPattern, GetStatReplacement);
		res = Regex.Replace(res, spritePattern, GetSpriteReplacement);
		return res;
	}

	public string Resolve(in ControllerInfo controllerModelInfo)
	{
		this.modelInfo = controllerModelInfo;
		return Resolve();
	}

	private string GetStatReplacement(Match match)
	{
		return GameStatisticManager.instance.GetStat(match.Groups[1].ToString());
	}

	private string GetSpriteReplacement(Match match)
	{
		string spriteName = match.Groups[1].ToString();
		if (!this.modelInfo.HasValue)
		{
			string errorMessage = "Cannot resolve player's controller, ControllerModel wasn't provided to the Resolve function. Please use the appropriate overload, use the default keyboard ControllerModel";
			LogManager.instance.AddLog(errorMessage, new object[] { text });
			Debug.Log(errorMessage);
            this.modelInfo = ControllerInfo.defaultModelInfo;
        }

		ControllerInfo modelInfo = this.modelInfo.Value;
		if(spriteName == "PlayerController")
		{
            string controllerName = modelInfo.controllerType == ControllerType.Keyboard ? "keyboard" : "gamepad";
            return $"<sprite=\"{SPRITESHEET}\" name=\"{controllerName}\">";
        }

		if(inputIconsKeywords.Contains(spriteName))
		{
            bool isKeyboard = modelInfo.controllerType == ControllerType.Keyboard;
			Sprite sprite;
			if(modelInfo.inputsKeys.TryGetValue(spriteName, out InputKey inputKey))
			{
                sprite = isKeyboard ? InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, inputKey) :
						InputIconManager.instance.GetButtonSprite(modelInfo.controlerModel, (InputKey)InputManager.ConvertGamepadKeyToGeneralGamepadKey((GamepadKey)inputKey));
            }
			else
			{
				string errorMessage = $"Error during resolving, Find the InputIcon keyword:{spriteName}, but the given modelInfo dictionnary have not this key!";
				Debug.LogWarning(errorMessage);
				LogManager.instance.AddLog(errorMessage, new object[] { modelInfo.controllerType, modelInfo.controlerModel, modelInfo.inputsKeys });
				return string.Empty;
            }
			return $"<sprite=\"{SPRITESHEET}\" name=\"{sprite.name}\">";
        }

        // By default, we consider that we got a hardcoded value
        return $"<sprite=\"{SPRITESHEET}\" name=\"{spriteName}\">";
	}

	public struct ControllerInfo
	{
		public static ControllerInfo defaultModelInfo => new ControllerInfo(ControllerType.Keyboard, new Dictionary<string, InputKey>(1), ControllerModel.Keyboard);

        public ControllerType controllerType;
        public ControllerModel controlerModel;
		public Dictionary<string, InputKey> inputsKeys;

		public ControllerInfo(ControllerType controllerType, Dictionary<string, InputKey> inputsKeys, ControllerModel controlerModel)
		{
			if (controllerType == ControllerType.Keyboard && controlerModel != ControllerModel.Keyboard && controlerModel != ControllerModel.None)
			{
				string errorMessage = "Can't have a Keyboard controller type and a Gamepad Controller model, replace it to a Keyboard controller model.";
				Debug.LogWarning(errorMessage);
				LogManager.instance.AddLog(errorMessage, new object[] { controllerType.ToString(), controlerModel.ToString() });
				controlerModel = ControllerModel.Keyboard;
			}

			if (controllerType == ControllerType.Any)
			{
				string errorMessage = "Can't have an \"any\" controller type, replace it to a Keyboard controller type.";
				Debug.LogWarning(errorMessage);
				LogManager.instance.AddLog(errorMessage, new object[] { controllerType.ToString(), controlerModel.ToString() });
				controllerType = ControllerType.Keyboard;
				controlerModel = ControllerModel.Keyboard;
			}

			if(controllerType == ControllerType.Keyboard)
			{
				List<string> elementToRemove = new List<string>(inputsKeys.Count);
                foreach (KeyValuePair<string, InputKey> item in inputsKeys)
				{
					if(!InputManager.IsKeyboardKey(item.Value))
					{
                        string errorMessage = $"Can't have a keyboard controller with gamepad key! Remove the key:{item.Value}";
                        Debug.LogWarning(errorMessage);
                        LogManager.instance.AddLog(errorMessage, new object[] { controllerType.ToString(), controlerModel.ToString(), inputsKeys });
						elementToRemove.Add(item.Key);
                    }
				}

				foreach (string key in elementToRemove)
				{
					inputsKeys.Remove(key);
				}
			}


            if (controllerType != ControllerType.Keyboard)
            {
                List<string> elementToRemove = new List<string>(inputsKeys.Count);
                foreach (KeyValuePair<string, InputKey> item in inputsKeys)
                {
                    if (!InputManager.IsGamepadKey(item.Value))
                    {
                        string errorMessage = $"Can't have a gamepad controller with Keyboard key! Remove the key:{item.Value}";
                        Debug.LogWarning(errorMessage);
                        LogManager.instance.AddLog(errorMessage, new object[] { controllerType.ToString(), controlerModel.ToString(), inputsKeys });
                        elementToRemove.Add(item.Key);
                    }
                }

                foreach (string key in elementToRemove)
                {
                    inputsKeys.Remove(key);
                }
            }

            this.controllerType = controllerType;
			this.controlerModel = controlerModel;
			this.inputsKeys = inputsKeys;
		}
    }
}
