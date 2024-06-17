using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintCampaign))]
[AllowMultipleComponents]
[TypeId("7b8079d29f714747b7d8d580d7a76710")]
public class BlueprintCampaignOverrideSettingBool : BlueprintCampaignOverrideSetting
{
	public UISettingsEntityBool Bool;

	public bool Value;

	public override void Activate()
	{
		Bool.Setting.OverrideStart(Value);
	}

	public override void Deactivate()
	{
		Bool.Setting.OverrideStop();
	}
}
