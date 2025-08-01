using System.Text.RegularExpressions;
using UnityEngine;

public class GameText
{
	private string text;
	// Stuff used by Resolve
	private ControllerModel model;

	private const string KEYBOARD_SPRITESHEET = "keyboard_keys";
	private const string GAMEPAD_SPRITESHEET = "xbox_keys";

	public GameText(string text)
	{
		this.text = text;
		this.model = ControllerModel.None;
	}

	public string Resolve()
	{
		const string statPattern = @"\$stat=([\w\d]+?)\$"; // Ex: "SuperSpell has an impedance of only $stat=superspell_impedance$ !"
		const string spritePattern = @"\$sprite=([\w\d]+?)\$"; // Ex: "Press $sprite=Key_Esc$ to pause"

		string res = text;
		res = Regex.Replace(res, statPattern, GetStatReplacement);
		res = Regex.Replace(res, spritePattern, GetSpriteReplacement);
		return res;
	}

	public string Resolve(ControllerModel model)
	{
		this.model = model;
		return Resolve();
	}

	private string GetStatReplacement(Match m)
	{
		return GameStatisticManager.instance.GetStat(m.Groups[1].ToString());
	}

	private string GetSpriteReplacement(Match m)
	{
		string spriteName = m.Groups[1].ToString();
		if (model == ControllerModel.None)
		{
			string errorMessage = "Cannot resolve player's controller, ControllerModel wasn't provided to the Resolve function. Please use the appropriate overload, use the default keyboard ControllerModel";
			LogManager.instance.AddLog(errorMessage, new object[] { text });
			Debug.Log(errorMessage);
            model = ControllerModel.Keyboard;
        }

        bool isKeyboard = model == ControllerModel.Keyboard;
		string spritesheet = isKeyboard ? KEYBOARD_SPRITESHEET : GAMEPAD_SPRITESHEET;
		Sprite sprite;

		switch (spriteName)
		{
			case "PlayerController":
				return isKeyboard ? $"<sprite=\"{KEYBOARD_SPRITESHEET}\" name=\"keyboard\">" : $"<sprite=\"{GAMEPAD_SPRITESHEET}\" name=\"gamepad\">";

			case "Key_Back":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.Escape) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPB);
				return $"<sprite=\"{spritesheet}\" name=\"{sprite.name}\">";

			case "Key_Validate":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.Return) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPA);
				return $"<sprite=\"{spritesheet}\" name=\"{sprite.name}\">";

			case "Key_Left":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.LeftArrow) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPDPadLeft);
				return $"<sprite=\"{spritesheet}\" name=\"{sprite.name}\">";

			case "Key_Right":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.RightArrow) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPDPadRight);
				return $"<sprite=\"{spritesheet}\" name=\"{sprite.name}\">";

			default:
				// By default, we consider that we got a hardcoded value
				return $"<sprite=\"{spritesheet}\" name=\"{spriteName}\">";
		}
	}
}
