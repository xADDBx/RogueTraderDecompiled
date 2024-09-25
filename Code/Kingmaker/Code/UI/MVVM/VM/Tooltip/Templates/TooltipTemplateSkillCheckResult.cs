using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Controllers.Dialog;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSkillCheckResult : TooltipBaseTemplate
{
	private readonly List<SkillCheckResult> m_SkillCheckResults;

	private readonly string[] m_KeyWords;

	public TooltipTemplateSkillCheckResult(List<SkillCheckResult> skillCheckResults, string[] keyWords)
	{
		m_SkillCheckResults = skillCheckResults;
		m_KeyWords = keyWords;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield break;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		UISkillcheckTooltip tooltips = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.SkillcheckTooltips;
		foreach (SkillCheckResult check in m_SkillCheckResults)
		{
			if (check != null)
			{
				yield return new TooltipBrickPortraitAndName(check.ActingUnit.Portrait.SmallPortrait, check.ActingUnit.CharacterName, new TooltipBrickTitle($"{UIUtilityTexts.GetSkillCheckName(check.StatType)}: {check.StatValue}", TooltipTitleType.H6, TextAlignmentOptions.Left));
				yield return new TooltipBrickChance(tooltips.SkillCheckChance.Text, check.TotalSkill, check.RollResult, 0, isResultValue: false, null, isProtectionIcon: false, isTargetHitIcon: false, isBorderChanceIcon: false, isGrayBackground: true);
				yield return new TooltipBrickTextValue(tooltips.SkillValue.Text, check.StatValue.ToString("+#;-#;0"), 1);
				if (check.DC != 0)
				{
					yield return new TooltipBrickTextValue(tooltips.DifficultyClass.Text, check.DC.ToString("+#;-#;0"), 1);
				}
			}
		}
		string[] keyWords = m_KeyWords;
		foreach (string key in keyWords)
		{
			BlueprintEncyclopediaGlossaryEntry glossaryEntry = UIUtility.GetGlossaryEntry(key);
			if (glossaryEntry != null)
			{
				yield return new TooltipBrickTitle(glossaryEntry.Title);
				yield return new TooltipBrickText(glossaryEntry.GetDescription(), TooltipTextType.Paragraph);
				break;
			}
		}
	}
}
