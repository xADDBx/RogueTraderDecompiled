using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities.Persistence.Actions;

public interface ISettingsUpgradeAction
{
	void Upgrade(ISettingsProvider provider);
}
