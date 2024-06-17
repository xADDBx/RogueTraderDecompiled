using System.Collections.Generic;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSliderVM : TooltipBaseBrickVM
{
	public AutoDisposingList<BrickSliderValueVM> SliderValueVMs = new AutoDisposingList<BrickSliderValueVM>();

	public int MaxValue;

	public int Value;

	public int PreferredHeight;

	public bool ShowValue;

	public Color32 FillColor;

	public LocalizedString MaxValueText;

	public TooltipBrickSliderVM(List<BrickSliderValueVM> sliderValueVMs, int maxValue, int value, bool showValue, int preferredHeight = 50, Color fillColor = default(Color), LocalizedString maxValueText = null)
	{
		SliderValueVMs.AddRange(sliderValueVMs);
		MaxValue = maxValue;
		Value = value;
		PreferredHeight = preferredHeight;
		ShowValue = showValue;
		if (fillColor == default(Color))
		{
			FillColor = new Color32(91, 54, 91, byte.MaxValue);
			return;
		}
		FillColor = fillColor;
		MaxValueText = maxValueText;
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		SliderValueVMs.Clear();
	}
}
