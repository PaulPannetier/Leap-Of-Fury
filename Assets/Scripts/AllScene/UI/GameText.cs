using System.Text.RegularExpressions;
using UnityEngine;

public class GameText
{
    private const string SPRITESHEET = "keys";

    private string text;
	private ControllerModel model;

	public GameText(string text)
	{
		this.text = text;
		this.model = ControllerModel.None;
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

	public string Resolve(ControllerModel model)
	{
		this.model = model;
		return Resolve();
	}

	private string GetStatReplacement(Match match)
	{
		return GameStatisticManager.instance.GetStat(match.Groups[1].ToString());
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
		Sprite sprite;

		switch (spriteName)
		{
			case "PlayerController":
				string controllerName = isKeyboard ? "keyboard" : "gamepad";
				return $"<sprite=\"{SPRITESHEET}\" name=\"{controllerName}\">";

			case "Key_Back":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.Escape) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPB);
				return $"<sprite=\"{SPRITESHEET}\" name=\"{sprite.name}\">";

			case "Key_Validate":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.Return) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPA);
				return $"<sprite=\"{SPRITESHEET}\" name=\"{sprite.name}\">";

			case "Key_Left":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.LeftArrow) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPDPadLeft);
				return $"<sprite=\"{SPRITESHEET}\" name=\"{sprite.name}\">";

			case "Key_Right":
				sprite = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.RightArrow) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPDPadRight);
				return $"<sprite=\"{SPRITESHEET}\" name=\"{sprite.name}\">";

			default:
				// By default, we consider that we got a hardcoded value
				return $"<sprite=\"{SPRITESHEET}\" name=\"{spriteName}\">";
		}
	}
}
