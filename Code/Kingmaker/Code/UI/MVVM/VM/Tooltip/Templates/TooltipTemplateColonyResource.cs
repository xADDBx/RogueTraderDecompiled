using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyResource : TooltipBaseTemplate
{
	[CanBeNull]
	public string ResourceName { get; }

	[CanBeNull]
	public string ResourceDescription { get; }

	[CanBeNull]
	public BlueprintResource BlueprintResource { get; }

	[CanBeNull]
	public int TotalCount { get; }

	public TooltipTemplateColonyResource(BlueprintResource blueprintResource, int count)
	{
		ResourceName = blueprintResource.Name;
		ResourceDescription = blueprintResource.Description;
		BlueprintResource = blueprintResource;
		TotalCount = count;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickIconStatValue(ResourceName, TotalCount.ToString(), null, null, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickText(ResourceDescription));
		SetStarSystemObjects(list);
		SetContracts(list);
		SetColonies(list);
		return list;
	}

	private void AddResourceSourcesGroup(List<ITooltipBrick> bricks, IEnumerable<ITooltipBrick> sources, string header)
	{
		bricks.Add(new TooltipBrickTitle(header, TooltipTitleType.H3));
		bricks.Add(new TooltipBricksGroupStart());
		bricks.AddRange(sources);
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void SetStarSystemObjects(List<ITooltipBrick> bricks)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		foreach (ColoniesState.MinerData miner in Game.Instance.Player.ColoniesState.Miners)
		{
			if (miner.Resource == BlueprintResource)
			{
				int resourceFromMinerCountWithProductivity = ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(miner);
				list.Add(new TooltipBrickIconStatValue(miner.Sso.Name, $"+{resourceFromMinerCountWithProductivity}", null, null, TooltipBrickIconStatValueType.Positive));
			}
		}
		if (!list.Empty())
		{
			AddResourceSourcesGroup(bricks, list, UIStrings.Instance.ExplorationTexts.ResourceMiner.Text);
		}
	}

	private void SetContracts(List<ITooltipBrick> bricks)
	{
		IEnumerable<BlueprintQuestContract> enumerable = from q in Game.Instance.Player.QuestBook.Quests
			where q is Contract
			where q.State == QuestState.Completed
			select q into c
			where c.Blueprint is BlueprintQuestContract
			select c into x
			select (BlueprintQuestContract)x.Blueprint;
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		foreach (BlueprintQuestContract item in enumerable)
		{
			foreach (RewardResourceNotFromColony item2 in item.GetComponents<RewardResourceNotFromColony>()?.EmptyIfNull())
			{
				if (item2.Resource == BlueprintResource)
				{
					list.Add(new TooltipBrickIconStatValue(item.Name, $"+{item2.Count}", null, null, TooltipBrickIconStatValueType.Positive));
				}
			}
			foreach (RequirementResourceUseOrder item3 in item.GetComponents<RequirementResourceUseOrder>()?.EmptyIfNull())
			{
				if (item3.ResourceBlueprint == BlueprintResource)
				{
					list.Add(new TooltipBrickIconStatValue(item.Name, $"-{item3.Count}", null, null, TooltipBrickIconStatValueType.Negative));
				}
			}
		}
		if (!list.Empty())
		{
			AddResourceSourcesGroup(bricks, list, UIStrings.Instance.QuesJournalTexts.Orders.Text);
		}
	}

	private void SetColonies(List<ITooltipBrick> bricks)
	{
		List<ColoniesState.ColonyData> colonies = Game.Instance.Player.ColoniesState.Colonies;
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		foreach (ColoniesState.ColonyData item in colonies)
		{
			int num = 0;
			foreach (KeyValuePair<BlueprintResource, int> item2 in from res in item.Colony.ProducedResourcesByColony()
				where res.Key == BlueprintResource
				select res)
			{
				num += item2.Value;
			}
			foreach (ColonyChronicle chronicle in item.Colony.Chronicles)
			{
				num += (from reward in chronicle.Blueprint.GetComponents<RewardResourceNotFromColony>()
					where reward.Resource == BlueprintResource
					select reward).Sum((RewardResourceNotFromColony reward) => reward.Count);
			}
			if (num > 0)
			{
				list.Add(new TooltipBrickIconStatValue(item.Colony.Blueprint.Name, $"+{num}", null, null, TooltipBrickIconStatValueType.Positive));
			}
			int num2 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item3 in from res in item.Colony.RequiredResourcesForColony()
				where res.Key == BlueprintResource
				select res)
			{
				num2 += item3.Value;
			}
			if (num2 > 0)
			{
				list.Add(new TooltipBrickIconStatValue(item.Colony.Blueprint.Name, $"-{num2}", null, null, TooltipBrickIconStatValueType.Negative));
			}
		}
		if (!list.Empty())
		{
			AddResourceSourcesGroup(bricks, list, UIStrings.Instance.ColonyProjectsRewards.ColonyRewardsHeader.Text);
		}
	}
}
