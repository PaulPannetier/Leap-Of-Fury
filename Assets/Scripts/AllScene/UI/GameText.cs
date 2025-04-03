using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class GameText {
	private string text;
	// Stuff used by Resolve
	private PlayerIndex player;
	private ControllerModel model;

	private const string KEYBOARD_SPRITESHEET = "keyboard_keys";
	private const string GAMEPAD_SPRITESHEET = "xbox_keys";

	public GameText(string text){
		this.text = text;
		this.model = ControllerModel.None;
	}

	public string Resolve(){
		const string stat_pattern = @"\$stat=([\w\d]+?)\$"; // Ex: "SuperSpell has an impedance of only $stat=superspell_impedance$ !"
		const string sprite_pattern = @"\$sprite=([\w\d]+?)\$"; // Ex: "Press $sprite=Key_Esc$ to pause"

		string res = text;
		res = Regex.Replace(res, stat_pattern, GetStatReplacement);
		res = Regex.Replace(res, sprite_pattern, GetSpriteReplacement);
		return res;
	}

	public string Resolve(ControllerModel model){
		this.model = model;
		return Resolve();
	}

	private string GetStatReplacement(Match m){
		return GameStatisticManager.instance.GetStat(m.Groups[1].ToString());
	}

	private string GetSpriteReplacement(Match m){
		string sprite_name = m.Groups[1].ToString();

		if (model == ControllerModel.None)
			throw new Exception("Cannot resolve player's controller, ControllerModel wasn't provided to the Resolve function. Please use the appropriate overload");

		bool isKeyboard = model == ControllerModel.Keyboard;
		string spritesheet = isKeyboard ? KEYBOARD_SPRITESHEET : GAMEPAD_SPRITESHEET;
		Sprite s;

		switch (sprite_name) {
			case "PlayerController":

				return isKeyboard ?
					$"<sprite=\"{KEYBOARD_SPRITESHEET}\" name=\"keyboard\">" : $"<sprite=\"{GAMEPAD_SPRITESHEET}\" name=\"gamepad\">";

			case "Key_Back":
				s = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.Escape) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPB);
				return $"<sprite=\"{spritesheet}\" name=\"{s.name}\">";

			case "Key_Validate":
				s = isKeyboard ?
					InputIconManager.instance.GetButtonSprite(BaseController.Keyboard, InputKey.Return) :
					InputIconManager.instance.GetButtonSprite(BaseController.Gamepad, InputKey.GPA);
				return $"<sprite=\"{spritesheet}\" name=\"{s.name}\">";

			default:
				// By default, we consider that we got a hardcoded value
				return $"<sprite=\"{spritesheet}\" name=\"{sprite_name}\">";
		}
	}
}
