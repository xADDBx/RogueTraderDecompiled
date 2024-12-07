using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyProject : TooltipBaseTemplate
{
	private readonly BlueprintColonyProject m_BlueprintColonyProject;

	private readonly Colony m_Colony;

	public TooltipTemplateColonyProject(BlueprintColonyProject blueprintColonyProject, Colony colony)
	{
		m_BlueprintColonyProject = blueprintColonyProject;
		m_Colony = colony;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_BlueprintColonyProject.Name, TooltipTitleType.H2, TextAlignmentOptions.Left);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		ColonyProject colonyProject = m_Colony.Projects.FirstOrDefault((ColonyProject proj) => proj.Blueprint == m_BlueprintColonyProject);
		if (colonyProject != null && m_BlueprintColonyProject.SegmentsToBuild != 0)
		{
			int num = Mathf.Clamp(Mathf.FloorToInt((float)(Game.Instance.TimeController.GameTime - colonyProject.StartTime).TotalSegments() / (float)m_Colony.SegmentsToBuildProject(m_BlueprintColonyProject) * 100f), 0, 100);
			list.Add(new TooltipBrickColonyProjectProgress(UIConfig.Instance.UIIcons.TooltipIcons.Duration, string.Format(UIStrings.Instance.ColonyProjectsTexts.BuildingInProgress.Text, num.ToString())));
		}
		list.Add(new TooltipBricksGroupStart());
		list.Add(new TooltipBrickText(UIStrings.Instance.QuesJournalTexts.RewardsResources, TooltipTextType.Bold, isHeader: false, TooltipTextAlignment.Left));
		foreach (Reward component in m_BlueprintColonyProject.GetComponents<Reward>())
		{
			RewardUI reward = RewardUIFactory.GetReward(component);
			if (!(component is RewardItem))
			{
				if (!(component is RewardResourceNotFromColony) && !(component is RewardResourceProject))
				{
					if (!(component is RewardChangeStatContentment) && !(component is RewardChangeStatEfficiency) && !(component is RewardChangeStatSecurity))
					{
						if (component is RewardProfitFactor)
						{
							list.Add(new TooltipBrickPFIconAndName(reward.Icon, reward.Description));
						}
						else
						{
							list.Add(new TooltipBrickText(reward.Description, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
						}
					}
					else
					{
						list.Add(new TooltipBrickResourceIconAndName(reward.Icon, reward.Description));
					}
				}
				else
				{
					list.Add(new TooltipBrickResourceIconAndName(reward.Icon, reward.Description));
				}
			}
			else
			{
				list.Add(new TooltipBrickItemIconAndName(reward.Icon, reward.Description));
			}
		}
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBricksGroupStart());
		list.Add(new TooltipBrickText(UIStrings.Instance.QuesJournalTexts.RequiredResources, TooltipTextType.Bold, isHeader: false, TooltipTextAlignment.Left));
		foreach (Requirement component2 in m_BlueprintColonyProject.GetComponents<Requirement>())
		{
			RequirementUI requirement = RequirementUIFactory.GetRequirement(component2);
			if (!(component2 is RequirementProfitFactorCost) && !(component2 is RequirementProfitFactorMinimum))
			{
				if (component2 is RequirementResourceHave || component2 is RequirementResourceUseOrder || component2 is RequirementResourceUseProject)
				{
					list.Add(new TooltipBrickResourceIconAndName(requirement.Icon, requirement.Description));
				}
				else
				{
					list.Add(new TooltipBrickText(requirement.Description, TooltipTextType.Simple, isHeader: false, TooltipTextAlignment.Left));
				}
			}
			else
			{
				list.Add(new TooltipBrickPFIconAndName(requirement.Icon, requirement.Description));
			}
		}
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBrickText(m_BlueprintColonyProject.Description, TooltipTextType.Italic));
		return list;
	}
}
