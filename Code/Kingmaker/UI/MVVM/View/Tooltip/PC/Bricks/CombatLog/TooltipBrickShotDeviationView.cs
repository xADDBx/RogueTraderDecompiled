using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickShotDeviationView : TooltipBaseBrickView<TooltipBrickShotDirectionVM>
{
	[SerializeField]
	private Slider m_DeviationValueSlider;

	[SerializeField]
	private RectTransform m_CentralDeviationAnchorSlider;

	[SerializeField]
	private RectTransform m_SlightDeviationAnchorSlider;

	[SerializeField]
	private RectTransform m_FarDeviationAnchorSlider;

	[SerializeField]
	private TextMeshProUGUI m_DeviationMinValueText;

	[SerializeField]
	private TextMeshProUGUI m_DeviationMaxValueText;

	[SerializeField]
	private TextMeshProUGUI m_DeviationValueText;

	protected override void BindViewImplementation()
	{
		m_CentralDeviationAnchorSlider.anchorMax = new Vector2((float)base.ViewModel.DeviationMin / 100f, 1f);
		m_SlightDeviationAnchorSlider.anchorMin = new Vector2((float)base.ViewModel.DeviationMin / 100f, 1f);
		m_SlightDeviationAnchorSlider.anchorMax = new Vector2((float)base.ViewModel.DeviationMax / 100f, 1f);
		m_FarDeviationAnchorSlider.anchorMin = new Vector2((float)base.ViewModel.DeviationMax / 100f, 1f);
		TextMeshProUGUI deviationMinValueText = m_DeviationMinValueText;
		int deviationMin = base.ViewModel.DeviationMin;
		deviationMinValueText.text = deviationMin.ToString();
		TextMeshProUGUI deviationMaxValueText = m_DeviationMaxValueText;
		deviationMin = base.ViewModel.DeviationMax;
		deviationMaxValueText.text = deviationMin.ToString();
		m_DeviationValueSlider.value = base.ViewModel.DeviationValue;
		TextMeshProUGUI deviationValueText = m_DeviationValueText;
		deviationMin = base.ViewModel.DeviationValue;
		deviationValueText.text = deviationMin.ToString();
	}
}
