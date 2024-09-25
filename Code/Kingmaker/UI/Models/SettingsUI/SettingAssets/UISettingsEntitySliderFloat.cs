using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/Slider Float")]
public class UISettingsEntitySliderFloat : UISettingsEntitySlider<float>
{
	[SerializeField]
	[FormerlySerializedAs("Step")]
	private float m_Step;

	[SerializeField]
	[FormerlySerializedAs("DecimalPlaces")]
	private int m_DecimalPlaces;

	public override bool IsInt => false;

	public override float Step => m_Step;

	public override int DecimalPlaces => m_DecimalPlaces;

	public override float GetFloatTempValue()
	{
		float tempValue = GetTempValue();
		if (tempValue < base.MinValue || tempValue > base.MaxValue)
		{
			Debug.LogError($"{tempValue} in {base.name} is out of Range!");
		}
		return tempValue;
	}

	public override void SetFloatTempValue(float value)
	{
		if (value < base.MinValue || value > base.MaxValue)
		{
			Debug.LogError($"{value} in {base.name} is out of Range!");
		}
		SetTempValue(value);
	}
}
