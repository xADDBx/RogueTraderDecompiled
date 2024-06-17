using System;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

public interface IUISettingsEntitySlider : IUISettingsEntityWithValueBase, IUISettingsEntityBase
{
	bool IsInt { get; }

	float Step { get; }

	bool ShowValueText { get; }

	int DecimalPlaces { get; }

	float MinValue { get; }

	float MaxValue { get; }

	bool IsPercentage { get; }

	bool ChangeDirection { get; }

	event Action<float> OnTempFloatValueChanged;

	float GetFloatTempValue();

	void SetFloatTempValue(float value);
}
