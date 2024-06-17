using System.Collections.Generic;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateLevelExp : TooltipBaseTemplate
{
	private readonly CharInfoExperienceVM m_ExperienceVM;

	private readonly BlueprintEncyclopediaGlossaryEntry m_LevelGlossaryEntry;

	private readonly TooltipStringsFormat m_TooltipStrings = new TooltipStringsFormat();

	public TooltipTemplateLevelExp(CharInfoExperienceVM experienceVM)
	{
		m_ExperienceVM = experienceVM;
		m_LevelGlossaryEntry = UIUtility.GetGlossaryEntry("CharacterLevel");
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_LevelGlossaryEntry?.Title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		int value = m_ExperienceVM.CurrentExp.Value;
		int value2 = m_ExperienceVM.NextLevelExp.Value;
		int num = value2 - value;
		list.Add(new TooltipBricksGroupStart());
		list.Add(new TooltipBrickIconStatValue(tooltips.CurrentLevelExperience, $"{value}"));
		list.Add(new TooltipBrickIconStatValue(tooltips.NextLevelExperience, $"{value2}"));
		if (num > 0)
		{
			list.Add(new TooltipBrickIconStatValue(tooltips.TillNextLevelExperience, $"{num}"));
		}
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBrickSpace());
		list.Add(new TooltipBrickText(m_LevelGlossaryEntry?.GetDescription()));
		return list;
	}
}
