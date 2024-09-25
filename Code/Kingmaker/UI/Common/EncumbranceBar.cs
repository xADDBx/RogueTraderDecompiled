using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UnitLogic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Common;

public class EncumbranceBar : MonoBehaviour
{
	[SerializeField]
	protected Slider m_CurrentLoadSlider;

	[SerializeField]
	protected Slider m_LightLoadSlider;

	[SerializeField]
	protected Slider m_MediumLoadSlider;

	[SerializeField]
	protected TextMeshProUGUI m_LightLoadLabel;

	[SerializeField]
	protected TextMeshProUGUI m_MediumLoadLabel;

	[SerializeField]
	protected TextMeshProUGUI m_HeavyLoadLabel;

	[SerializeField]
	protected TextMeshProUGUI m_CurrentLoadStatus;

	[SerializeField]
	protected TextMeshProUGUI m_CurrentLoadValue;

	[SerializeField]
	protected Color m_LightColor;

	[SerializeField]
	protected Color m_MiddleColor;

	[SerializeField]
	protected Color m_HeavyColor;

	[SerializeField]
	protected Color m_OwerloadColor;

	[SerializeField]
	protected Graphic m_FillGraphic;

	[Header("Only debug")]
	public Gradient g;

	public void SetCapacity(EncumbranceHelper.CarryingCapacity capacity)
	{
		float time = Mathf.Clamp01(capacity.CurrentWeight / (float)capacity.Heavy);
		g = new Gradient();
		g.mode = GradientMode.Blend;
		g.colorKeys = new GradientColorKey[4]
		{
			new GradientColorKey(m_LightColor, 0f),
			new GradientColorKey(m_MiddleColor, (float)capacity.Medium / (float)capacity.Heavy),
			new GradientColorKey(m_HeavyColor, 0.9f),
			new GradientColorKey(m_OwerloadColor, 1f)
		};
		m_FillGraphic.color = g.Evaluate(time);
		m_LightLoadLabel.text = capacity.Light.ToString();
		m_MediumLoadLabel.text = capacity.Medium.ToString();
		m_HeavyLoadLabel.text = capacity.Heavy.ToString();
		m_LightLoadSlider.maxValue = capacity.Heavy;
		m_LightLoadSlider.value = capacity.Light;
		m_MediumLoadSlider.maxValue = capacity.Heavy;
		m_MediumLoadSlider.value = capacity.Medium;
		m_CurrentLoadSlider.maxValue = capacity.Heavy;
		m_CurrentLoadSlider.value = capacity.CurrentWeight;
		m_CurrentLoadStatus.text = capacity.GetEncumbranceText();
		m_CurrentLoadValue.text = $"{capacity.GetCurrentWeightText()} {UIStrings.Instance.Tooltips.lbs}.";
	}
}
