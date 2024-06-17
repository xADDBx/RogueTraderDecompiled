namespace Kingmaker.Settings.Entities;

public interface ISettingsEntityEnum
{
	int GetTempValue();

	void SetValueAndConfirm(int value);
}
