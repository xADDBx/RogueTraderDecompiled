using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings.LINQ;

public class WasTouchedSetting<TSource> : SettingsEntityBool
{
	private readonly TSource m_DefaultValue;

	public WasTouchedSetting(SettingsEntity<TSource> source)
		: base(source.settingsController, source.Key + "-was-touched", defaultValue: false, source.SaveDependent)
	{
		m_DefaultValue = source.DefaultValue;
		source.OnTempValueChanged += UpdateSourceTempValue;
		source.OnValueChanged += UpdateSourceValue;
	}

	private void UpdateSourceTempValue(TSource value)
	{
		if (!GetValue())
		{
			SetTempValue(!object.Equals(m_DefaultValue, value));
		}
	}

	private void UpdateSourceValue(TSource value)
	{
		if (!object.Equals(value, m_DefaultValue))
		{
			SetValueAndConfirm(value: true);
		}
	}
}
