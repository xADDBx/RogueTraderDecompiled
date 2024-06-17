using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings;

public static class SettingsHelper
{
	public static float CalculateCRModifier()
	{
		int num = (int)SettingsRoot.Difficulty.MaxCRScaling - (int)SettingsRoot.Difficulty.MinCRScaling;
		if (num != 0 && Game.Instance.CurrentlyLoadedArea != null)
		{
			return Math.Min((float)Game.Instance.CurrentlyLoadedArea.GetCR() / (float)num, 1f);
		}
		return 1f;
	}

	public static float CalculateCRModifier(SettingsEntityInt modifier)
	{
		if ((int)modifier < 0)
		{
			return 1f;
		}
		return CalculateCRModifier();
	}
}
