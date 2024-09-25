using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/ResetControllerMode")]
public class UISettingsEntityResetControllerMode : UISettingsEntityBase
{
	public override SettingsListItemType? Type => SettingsListItemType.Custom;
}
