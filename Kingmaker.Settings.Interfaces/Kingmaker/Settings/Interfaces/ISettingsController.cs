namespace Kingmaker.Settings.Interfaces;

public interface ISettingsController
{
	ISettingsProvider InSaveSettingsProvider { get; }

	ISettingsProvider GeneralSettingsProvider { get; }

	void AccountSetting(ISettingsEntity settingsEntity);

	void RemoveFromConfirmationList(ISettingsEntity settingsEntity, bool confirming);

	bool ConfirmationListContains(ISettingsEntity settingsEntity);

	void AddToConfirmationList(ISettingsEntity settingsEntity);

	void SaveAll();
}
