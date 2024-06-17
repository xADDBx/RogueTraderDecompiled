using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/Separator")]
public class UISettingsEntitySeparator : UISettingsEntityBase
{
	public override SettingsListItemType? Type => SettingsListItemType.Separator;
}
