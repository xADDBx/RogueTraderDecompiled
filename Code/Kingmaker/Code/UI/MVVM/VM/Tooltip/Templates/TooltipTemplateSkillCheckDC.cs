using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Controllers.Dialog;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSkillCheckDC : TooltipBaseTemplate
{
	private readonly List<SkillCheckDC> m_SkillCheckDcs;

	public TooltipTemplateSkillCheckDC(List<SkillCheckDC> skillCheckDcs)
	{
		m_SkillCheckDcs = skillCheckDcs;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		SkillCheckDC skillCheckDC = m_SkillCheckDcs.FirstOrDefault();
		if (skillCheckDC != null && skillCheckDC.ActingUnit != null && skillCheckDC.IsBestParameter && !skillCheckDC.FakePassed.HasValue)
		{
			yield return new TooltipBrickText(UIStrings.Instance.Tooltips.TipPreviewSkillcheckBestCharacter, TooltipTextType.Italic | TooltipTextType.Centered, isHeader: true, TooltipTextAlignment.Midl, needChangeSize: true, 16);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		UISkillcheckTooltip tooltips = UIStrings.Instance.SkillcheckTooltips;
		SkillCheckDC check = m_SkillCheckDcs.FirstOrDefault();
		if (check == null)
		{
			yield break;
		}
		if (check.ActingUnit != null)
		{
			yield return new TooltipBrickPortraitAndName(check.ActingUnit.Portrait.SmallPortrait, check.ActingUnit.CharacterName, new TooltipBrickTitle($"{UIUtilityTexts.GetSkillCheckName(check.StatType)}: {check.ValueDC}", TooltipTitleType.H6, TextAlignmentOptions.Left));
			yield return new TooltipBrickChance(tooltips.SkillCheckChance.Text, UIUtilityTexts.GetSkillCheckChance(check), check.FakeRoll, 0, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
			yield return new TooltipBrickTextValue(tooltips.SkillValue.Text, check.ValueDC.ToString("+#;-#;0"), 1);
			if (check.ConditionDC != 0)
			{
				yield return new TooltipBrickTextValue(tooltips.DifficultyClass.Text, check.ConditionDC.ToString("+#;-#;0"), 1);
			}
			yield break;
		}
		string title = string.Format(UIStrings.Instance.Tooltips.TitlePreviewSkillcheckSkillDC, UIStrings.Instance.SkillcheckTooltips.SkillCheck.Text, UIUtilityTexts.GetSkillCheckName(check.StatType));
		yield return new TooltipBrickTitle(title);
		yield return new TooltipBrickText(UIUtilityTexts.GetSkillCheckName(check.StatType) + ": " + UIUtility.AddSign(check.ValueDC) + "\n" + $"({UIStrings.Instance.SkillcheckTooltips.SkillCheckChance.Text}: {UIUtilityTexts.GetSkillCheckChance(check)}%)", TooltipTextType.Centered);
		yield return new TooltipBrickIconValueStat(tooltips.DifficultyModRoll, check.ConditionDC.ToString(), null, TooltipIconValueStatType.Centered);
		yield return new TooltipBrickValueStatFormula($"{check.ConditionDC.ToString()} {tooltips.SkillValue.Text} + {check.ValueDC} {tooltips.DifficultyClass.Text}", null, null);
	}
}
