using System.Collections.Generic;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSlider : ITooltipBrick
{
	private List<BrickSliderValueVM> m_SliderValueVMs = new List<BrickSliderValueVM>();

	private int m_MaxValue;

	private int m_Value;

	private int m_PreferredHeight;

	private bool m_ShowValue;

	private Color32 m_BaseColor;

	private Color32 m_FillColor;

	private LocalizedString M_MaxValueText;

	public TooltipBrickSlider(int maxValue, int value, List<BrickSliderValueVM> sliderValueVMs, bool showValue = false, int preferredHeight = 50, Color fillColor = default(Color), LocalizedString maxValueText = null)
	{
		m_SliderValueVMs = sliderValueVMs;
		m_MaxValue = maxValue;
		m_Value = value;
		m_ShowValue = showValue;
		m_PreferredHeight = preferredHeight;
		if (fillColor == default(Color))
		{
			m_FillColor = new Color32(91, 54, 91, byte.MaxValue);
			return;
		}
		m_FillColor = fillColor;
		M_MaxValueText = maxValueText;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSliderVM(m_SliderValueVMs, m_MaxValue, m_Value, m_ShowValue, m_PreferredHeight, m_FillColor, M_MaxValueText);
	}
}
