using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;
using Kingmaker.Settings.Entities;
using Kingmaker.TextTools.Base;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.TextTools;

public class KeyBindingTemplate : TextTemplate
{
	private const string UNKNOWN_BINDING = "<b><unknown binding></b>";

	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.Count < 1)
		{
			return "<b><unknown binding></b>";
		}
		UISettingsEntityKeyBinding bindByName = Game.Instance.UISettingsManager.GetBindByName(parameters[0]);
		if (bindByName != null)
		{
			KeyBindingData binding = bindByName.GetBinding(0);
			if (binding.Key != 0)
			{
				return binding.GetPrettyString();
			}
			binding = bindByName.GetBinding(1);
			if (binding.Key != 0)
			{
				return binding.GetPrettyString();
			}
		}
		return "<b><unknown binding></b>";
	}
}
