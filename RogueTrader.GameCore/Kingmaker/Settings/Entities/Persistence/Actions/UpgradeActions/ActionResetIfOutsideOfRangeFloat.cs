using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities.Persistence.Actions.UpgradeActions;

public class ActionResetIfOutsideOfRangeFloat : ISettingsUpgradeAction
{
	private readonly string m_Key;

	private readonly float m_Min;

	private readonly float m_Max;

	private readonly float m_Value;

	public ActionResetIfOutsideOfRangeFloat(string key, float min, float max, float value)
	{
		m_Key = key;
		m_Min = min;
		m_Max = max;
		m_Value = value;
	}

	public void Upgrade(ISettingsProvider provider)
	{
		if (provider.HasKey(m_Key))
		{
			float value = provider.GetValue<float>(m_Key);
			if (value < m_Min || value > m_Max)
			{
				provider.SetValue(m_Key, m_Value);
			}
		}
	}
}
