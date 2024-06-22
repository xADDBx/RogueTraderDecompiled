using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/On Off Toggle Only One Save")]
public class UISettingsEntityBoolOnlyOneSave : UISettingsEntityWithValueBase<bool>
{
	public bool DefaultValue;

	public override SettingsListItemType? Type => SettingsListItemType.OnOffToggle;
}
