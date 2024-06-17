using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateVail : TooltipBaseTemplate
{
	private readonly string m_GlossaryHeader;

	private readonly string m_GlossaryFooter;

	private int m_VailValue;

	public TooltipTemplateVail()
	{
		m_GlossaryHeader = UIStrings.Instance.ActionBar.VailHeader.Text;
		m_GlossaryFooter = UIStrings.Instance.ActionBar.VailFooter.Text;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickIconValueStat(UIUtility.GetGlossaryEntryName("VeilThickness"), m_VailValue.ToString(), BlueprintRoot.Instance.UIConfig.UIIcons.TooltipIcons.Vail, TooltipIconValueStatType.Normal, isWhite: true)
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickText(m_GlossaryHeader, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
		int maximumVeilOnAllLocation = BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.MaximumVeilOnAllLocation;
		int criticalVeilOnAllLocation = BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.CriticalVeilOnAllLocation;
		TooltipIcons tooltipIcons = BlueprintRoot.Instance.UIConfig.UIIcons.TooltipIcons;
		list.Add(new TooltipBrickSlider(maximumVeilOnAllLocation, m_VailValue, new List<BrickSliderValueVM>
		{
			new BrickSliderValueVM(maximumVeilOnAllLocation, criticalVeilOnAllLocation, tooltipIcons.BrokenVeil, needColor: true, UIConfig.Instance.TooltipColors.ProgressbarBonus)
		}, showValue: false, 50, UIConfig.Instance.TooltipColors.ProgressbarPenalty));
		list.Add(new TooltipBrickText(m_GlossaryFooter, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		if (type == TooltipTemplateType.Info)
		{
			list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			list.Add(new TooltipBrickTitle(UIStrings.Instance.ActionBar.VailCurrentState.Text, TooltipTitleType.H6, TextAlignmentOptions.Left));
			list.Add(new TooltipBrickText(UIStrings.Instance.ActionBar.VailStates.Text, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
			list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			list.Add(new TooltipBrickTitle(UIStrings.Instance.ActionBar.StatesOfVail.Text, TooltipTitleType.H6, TextAlignmentOptions.Left));
			list.Add(new TooltipBrickText(string.Format(UIStrings.Instance.ActionBar.BrokenVeil.Text, criticalVeilOnAllLocation, maximumVeilOnAllLocation), TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
		}
		return list;
	}

	public void ChangeValue(int val)
	{
		m_VailValue = val;
	}
}
