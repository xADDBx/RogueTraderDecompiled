using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickDamageRangeView : TooltipBrickCombatLogBaseView<TooltipBrickDamageRangeVM>
{
	[Header("Slider")]
	[SerializeField]
	private Slider m_CurrentValueSlider;

	[SerializeField]
	private TextMeshProUGUI m_CurrentValueText;

	[SerializeField]
	private TextMeshProUGUI m_MinValueText;

	[SerializeField]
	private TextMeshProUGUI m_MaxValueText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CurrentValueSlider.minValue = base.ViewModel.MinValue;
		m_CurrentValueSlider.maxValue = base.ViewModel.MaxValue;
		m_CurrentValueSlider.value = base.ViewModel.CurrentValue;
		TextMeshProUGUI currentValueText = m_CurrentValueText;
		int currentValue = base.ViewModel.CurrentValue;
		currentValueText.text = currentValue.ToString();
		TextMeshProUGUI minValueText = m_MinValueText;
		currentValue = base.ViewModel.MinValue;
		minValueText.text = currentValue.ToString();
		TextMeshProUGUI maxValueText = m_MaxValueText;
		currentValue = base.ViewModel.MaxValue;
		maxValueText.text = currentValue.ToString();
	}
}
