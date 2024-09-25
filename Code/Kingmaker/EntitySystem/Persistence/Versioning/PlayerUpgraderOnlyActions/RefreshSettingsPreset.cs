using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("0d414b10750b40488c64930f0d948f8a")]
public class RefreshSettingsPreset : PlayerUpgraderOnlyAction
{
	protected override void RunActionOverride()
	{
		GameDifficultyOption currentDifficulty = SettingsRoot.Difficulty.GameDifficulty.GetValue();
		if (currentDifficulty != GameDifficultyOption.Custom && BlueprintRoot.Instance.SettingsValues.DifficultiesPresets.Difficulties.TryFind((DifficultyPresetAsset x) => x.Preset.GameDifficulty == currentDifficulty, out var result))
		{
			SettingsController.Instance.DifficultyPresetsController.SetDifficultyPreset(result.Preset, confirm: true);
		}
	}

	public override string GetCaption()
	{
		return "Refresh all settings from preset (if not custom)";
	}
}
