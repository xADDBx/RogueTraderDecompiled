using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings.LINQ;

public class SelectSetting<TSource, TValue> : IReadOnlySettingEntity<TValue>
{
	private Func<TSource, TValue> m_Converter;

	private TValue m_Value;

	private TValue m_TempValue;

	public event Action<TValue> OnTempValueChanged;

	public event Action<TValue> OnValueChanged;

	public SelectSetting(IReadOnlySettingEntity<TSource> source, Func<TSource, TValue> converter)
	{
		m_Converter = converter;
		source.OnValueChanged += UpdateValue;
		source.OnTempValueChanged += UpdateTempValue;
		m_Value = m_Converter(source.GetValue());
		m_TempValue = m_Converter(source.GetTempValue());
	}

	private void UpdateValue(TSource sourceValue)
	{
		m_Value = m_Converter(sourceValue);
		this.OnValueChanged?.Invoke(m_Value);
	}

	private void UpdateTempValue(TSource sourceValue)
	{
		m_TempValue = m_Converter(sourceValue);
		this.OnTempValueChanged?.Invoke(m_TempValue);
	}

	public TValue GetValue()
	{
		return m_Value;
	}

	public TValue GetTempValue()
	{
		return m_TempValue;
	}
}
