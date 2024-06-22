using System;
using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings;

public static class SettingsHelper
{
	public static float CalculateCRModifier()
	{
		int num = (int)SettingsRoot.Difficulty.MaxCRScaling - (int)SettingsRoot.Difficulty.MinCRScaling;
		float? num2 = TryGetAreaCR();
		if (num == 0 || !num2.HasValue)
		{
			return 1f;
		}
		return Math.Min(num2.Value / (float)num, 1f);
	}

	public static float CalculateCRModifier(SettingsEntityInt modifier)
	{
		if ((int)modifier < 0)
		{
			return 1f;
		}
		return CalculateCRModifier();
	}

	private static int? TryGetAreaCR()
	{
		if (ContextData<BlueprintUnitCheckerInEditorContextData>.Current == null)
		{
			if (Game.Instance.CurrentlyLoadedArea == null)
			{
				return null;
			}
			return Game.Instance.CurrentlyLoadedArea.GetCR();
		}
		return ContextData<BlueprintUnitCheckerInEditorContextData>.Current?.AreaCR;
	}
}
