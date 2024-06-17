using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/GammaCorrection")]
public class UISettingsEntityGammaCorrection : UISettingsEntitySliderFloat
{
	public override SettingsListItemType? Type => SettingsListItemType.Custom;
}
