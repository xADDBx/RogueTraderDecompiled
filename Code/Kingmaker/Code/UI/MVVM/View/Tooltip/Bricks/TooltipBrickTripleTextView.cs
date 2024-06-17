using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks.Utils;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickTripleTextView : TooltipBrickDoubleTextView
{
	[Header("MiddlePart")]
	[SerializeField]
	private TextMeshProUGUI m_MiddleLabel;

	[SerializeField]
	private GameObject m_MiddleSide;

	[Header("Icons")]
	[SerializeField]
	private Image m_LeftIcon;

	[SerializeField]
	private Image m_MiddleIcon;

	[SerializeField]
	private Image m_RightIcon;

	protected override void BindViewImplementation()
	{
		if (TextHelper == null)
		{
			TextHelper = new AccessibilityTextHelper(m_LeftLabel, m_RightLabel, m_MiddleLabel);
		}
		m_LeftLabel.text = base.ViewModel.LeftLine;
		m_LeftSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.LeftLine));
		m_RightLabel.text = base.ViewModel.RightLine;
		m_RightSide.SetActive(!string.IsNullOrEmpty(base.ViewModel.RightLine));
		if (base.ViewModel is TooltipBrickTripleTextVM { MiddleLine: var middleLine } tooltipBrickTripleTextVM)
		{
			m_MiddleLabel.text = middleLine;
			m_MiddleSide.SetActive(!string.IsNullOrEmpty(middleLine));
			m_LeftIcon.gameObject.SetActive(tooltipBrickTripleTextVM.LeftIcon != null);
			m_LeftIcon.sprite = tooltipBrickTripleTextVM.LeftIcon;
			m_MiddleIcon.gameObject.SetActive(tooltipBrickTripleTextVM.MiddleIcon != null);
			m_MiddleIcon.sprite = tooltipBrickTripleTextVM.MiddleIcon;
			m_RightIcon.gameObject.SetActive(tooltipBrickTripleTextVM.RightIcon != null);
			m_RightIcon.sprite = tooltipBrickTripleTextVM.RightIcon;
			m_LeftLabel.ApplyTextFieldParams(tooltipBrickTripleTextVM.LeftParams);
			m_MiddleLabel.ApplyTextFieldParams(tooltipBrickTripleTextVM.MiddleParams);
			m_RightLabel.ApplyTextFieldParams(tooltipBrickTripleTextVM.RightParams);
		}
		TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		TextHelper.Dispose();
	}
}
