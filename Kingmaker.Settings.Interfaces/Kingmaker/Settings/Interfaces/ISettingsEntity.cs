using System;

namespace Kingmaker.Settings.Interfaces;

public interface ISettingsEntity
{
	bool SaveDependent { get; }

	bool TempValueIsConfirmed { get; }

	string Key { get; }

	event Action<bool> OnTempValueIsConfirmed;

	string GetStringValue();

	string GetStringDefaultValue();

	void ConfirmTempValue();

	void RevertTempValue();

	void SetCurrentValueInProvider();

	void ResetToDefault(bool confirmChanges);

	void ResetCache();

	void OnProviderValueChanged();

	bool CurrentValueIsNotDefault();
}
