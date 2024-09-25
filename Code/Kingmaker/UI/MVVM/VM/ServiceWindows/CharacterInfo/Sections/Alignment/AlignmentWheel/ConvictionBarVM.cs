using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.UnitLogic.Alignments;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentWheel;

public class ConvictionBarVM : CharInfoComponentVM
{
	private const float EdgeValue = 700f;

	public readonly FloatReactiveProperty CurrentRelativeValue = new FloatReactiveProperty();

	public readonly TooltipBaseTemplate RadicalTooltip;

	public readonly TooltipBaseTemplate PuritanTooltip;

	public readonly TooltipBaseTemplate CurrentTooltip;

	public ConvictionBarVM(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		UITextAlignment alignment = UIStrings.Instance.Alignment;
		RadicalTooltip = new TooltipTemplateSimple(alignment.RadicalTitle, alignment.RadicalDescription);
		PuritanTooltip = new TooltipTemplateSimple(alignment.PuritanTitle, alignment.PuritanDescription);
		CurrentTooltip = new TooltipTemplateSimple(alignment.CurrentConvictionTitle, alignment.CurrentConvictionDescription);
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		SoulMarkTooltipExtensions.GetSoulMarkInfo(SoulMarkShiftExtension.GetBaseSoulMarkFor(SoulMarkDirection.Faith), Unit.Value, out var rankThresholds, out var maxValue, out var currentValue, out var currentTier);
		SoulMarkTooltipExtensions.GetSoulMarkInfo(SoulMarkShiftExtension.GetBaseSoulMarkFor(SoulMarkDirection.Corruption), Unit.Value, out rankThresholds, out currentTier, out var currentValue2, out maxValue);
		SoulMarkTooltipExtensions.GetSoulMarkInfo(SoulMarkShiftExtension.GetBaseSoulMarkFor(SoulMarkDirection.Hope), Unit.Value, out rankThresholds, out maxValue, out var currentValue3, out currentTier);
		float value = (float)(currentValue2 + currentValue3 - currentValue) / 700f;
		value = Mathf.Clamp(value, -1f, 1f);
		CurrentRelativeValue.Value = value;
	}
}
