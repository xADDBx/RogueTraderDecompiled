using System;
using Kingmaker.Localization;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class BrickSliderValueVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly int Value;

	public readonly int MaxValue;

	public readonly LocalizedString Text;

	public readonly Sprite Sprite;

	public readonly bool IsValueOnBottom;

	public readonly bool NeedColor;

	public readonly Color32? ValueColor;

	public readonly Color32? TextColor;

	public readonly Color32 BgrColor;

	public BrickSliderValueVM(int maxValue, int value, Sprite sprite = null, bool needColor = false, Color32 bgrColor = default(Color32), Color32? valueColor = null, bool isValueOnBottom = true, LocalizedString text = null, Color32? textColor = null)
	{
		MaxValue = maxValue;
		Value = value;
		Sprite = sprite;
		NeedColor = needColor;
		BgrColor = bgrColor;
		ValueColor = valueColor;
		IsValueOnBottom = isValueOnBottom;
		Text = text;
		TextColor = textColor;
	}

	protected override void DisposeImplementation()
	{
	}
}
