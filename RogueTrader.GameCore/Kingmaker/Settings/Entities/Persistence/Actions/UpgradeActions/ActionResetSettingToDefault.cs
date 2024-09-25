using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities.Persistence.Actions.UpgradeActions;

public class ActionResetSettingToDefault : ISettingsUpgradeAction
{
	private readonly string m_Key;

	public ActionResetSettingToDefault(string key)
	{
		m_Key = key;
	}

	public void Upgrade(ISettingsProvider provider)
	{
		if (provider.HasKey(m_Key))
		{
			provider.RemoveKey(m_Key);
		}
	}
}
