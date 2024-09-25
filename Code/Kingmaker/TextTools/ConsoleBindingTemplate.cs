using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.TextTools.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

namespace Kingmaker.TextTools;

public class ConsoleBindingTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 2;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.Count < 1)
		{
			return "<b><unknown binding></b>";
		}
		TrimSpaces(parameters);
		if (Enum.TryParse<RewiredActionType>(parameters[0], ignoreCase: true, out var result))
		{
			if (GamePadIcons.Instance.GetIcon(result) == null)
			{
				return "<b><unknown binding></b>";
			}
			int num = UIConfig.Instance.DefaultConsoleHintScaleInText;
			if (parameters.Count == 2)
			{
				try
				{
					num = int.Parse(parameters[1]);
				}
				catch
				{
					num = 100;
				}
			}
			string arg = GamePad.Instance.Type switch
			{
				ConsoleType.PS4 => "UI_PS4_", 
				ConsoleType.PS5 => "UI_PS5_", 
				ConsoleType.XBox => "UI_XBox_", 
				ConsoleType.Switch => "UI_Switch_", 
				ConsoleType.SteamController => "UI_Steam_", 
				ConsoleType.SteamDeck => GetSteamDeckPrefix(result), 
				ConsoleType.Common => "UI_", 
				_ => "PS5_", 
			};
			if ((uint)result <= 24u)
			{
				string arg2 = result.ToString();
				int num2 = ((LocalizationManager.Instance.CurrentLocale == Locale.zhCN) ? (-20) : 0);
				return $"<size={num + num2}%><sprite name=\"{arg}{arg2}\"></size>";
			}
			throw new ArgumentOutOfRangeException();
		}
		return "<b><unknown binding></b>";
	}

	private void TrimSpaces(List<string> parameters)
	{
		for (int i = 0; i < parameters.Count; i++)
		{
			parameters[i] = parameters[i].Trim();
		}
	}

	private string GetSteamDeckPrefix(RewiredActionType actionType)
	{
		switch (actionType)
		{
		case RewiredActionType.Options:
		case RewiredActionType.FuncAdditional:
			return "UI_SteamDeck_";
		case RewiredActionType.LeftBottom:
		case RewiredActionType.RightBottom:
		case RewiredActionType.LeftUp:
		case RewiredActionType.RightUp:
		case RewiredActionType.LeftStickButton:
		case RewiredActionType.RightStickButton:
			return "UI_PS5_";
		case RewiredActionType.LeftStickX:
		case RewiredActionType.LeftStickY:
		case RewiredActionType.RightStickX:
		case RewiredActionType.RightStickY:
		case RewiredActionType.DPadLeft:
		case RewiredActionType.DPadRight:
		case RewiredActionType.DPadUp:
		case RewiredActionType.DPadDown:
		case RewiredActionType.Confirm:
		case RewiredActionType.Decline:
		case RewiredActionType.Func01:
		case RewiredActionType.Func02:
		case RewiredActionType.DPadFull:
		case RewiredActionType.DPadHorizontal:
		case RewiredActionType.DPadVertical:
		case RewiredActionType.LeftStick:
		case RewiredActionType.RightStick:
			return "UI_XBox_";
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
