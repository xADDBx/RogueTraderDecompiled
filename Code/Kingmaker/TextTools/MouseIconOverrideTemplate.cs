using System.Collections.Generic;
using Kingmaker.Settings;
using Kingmaker.TextTools.Base;
using Kingmaker.Utility;

namespace Kingmaker.TextTools;

public class MouseIconOverrideTemplate : TextTemplate
{
	private Dictionary<string, string> m_MouseIconsCommon = new Dictionary<string, string>
	{
		{ "LeftMouse", "UI_LeftMouseBTN" },
		{ "RightMouse", "UI_RightMouseBTN" },
		{ "MouseWheel", "UI_MouseWheel" }
	};

	private Dictionary<string, string> m_MouseIconsSwitch = new Dictionary<string, string>
	{
		{ "LeftMouse", "UI_Switch_RightUp" },
		{ "RightMouse", "UI_Switch_RightBottom" }
	};

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (!((ApplicationHelper.IsRunningOnSwitch2 && (bool)SettingsRoot.Game.Switch.SwitchJoyConAsMouse) ? m_MouseIconsSwitch : m_MouseIconsCommon).TryGetValue(parameters[0], out var value))
		{
			return string.Empty;
		}
		return "<sprite name=" + value + ">";
	}
}
