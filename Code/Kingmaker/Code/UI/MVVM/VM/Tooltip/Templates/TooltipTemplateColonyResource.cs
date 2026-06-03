using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyResource : TooltipBaseTemplate
{
	private ColonyResourceVM Combativity;

	[CanBeNull]
	public string ResourceName { get; }

	[CanBeNull]
	public string ResourceDescription { get; }

	[CanBeNull]
	public BlueprintResource BlueprintResource { get; }

	[CanBeNull]
	public int TotalCount { get; }

	public TooltipTemplateColonyResource(BlueprintResource blueprintResource, int count, ColonyResourceVM combativity)
	{
		ResourceName = blueprintResource.Name;
		ResourceDescription = blueprintResource.Description;
		BlueprintResource = blueprintResource;
		TotalCount = count;
		Combativity = combativity;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		ColonyResourceVM combativity2 = Combativity;
		if (combativity2 is CombativityResourceVM combativity)
		{
			yield return new TooltipBrickTitle(UIStrings.Instance.ProfitFactorTexts.CombativityTitle, TooltipTitleType.H1);
			string profitFactorFormatted = UIUtility.GetProfitFactorFormatted(combativity.TotalValue.Value);
			yield return new TooltipBrickIconStatValue(UIStrings.Instance.ProfitFactorTexts.TotalValue, profitFactorFormatted, null, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold);
		}
		else
		{
			yield return new TooltipBrickIconStatValue(ResourceName, TotalCount.ToString(), null, null, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueType.Normal, TooltipBrickIconStatValueStyle.Bold);
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		if (Combativity is CombativityResourceVM combativityResourceVM)
		{
			IEnumerable<ProfitFactorModifierVM> enumerable = combativityResourceVM.Modifiers.Where((ProfitFactorModifierVM mod) => mod.IsNegative);
			IEnumerable<ProfitFactorModifierVM> enumerable2 = combativityResourceVM.Modifiers.Except(enumerable);
			if (enumerable2.Any() || enumerable.Any())
			{
				list.Add(new TooltipBrickSpace());
			}
			AddModifiers(list, UIStrings.Instance.ProfitFactorTexts.Income, enumerable2, isPositive: true);
			AddModifiers(list, UIStrings.Instance.ProfitFactorTexts.Loss, enumerable, isPositive: false);
			list.Add(new TooltipBrickText(ResourceDescription));
			return list;
		}
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
			foreach (RewardResourceNotFromColony component in item.GetComponents<RewardResourceNotFromColony>())
			{
				if (component.Resource == BlueprintResource)
				{
					list.Add(new TooltipBrickIconStatValue(item.Name, $"+{component.Count}", null, null, TooltipBrickIconStatValueType.Positive));
				}
			}
			foreach (RequirementResourceUseOrder component2 in item.GetComponents<RequirementResourceUseOrder>())
			{
				if (component2.ResourceBlueprint == BlueprintResource)
				{
					list.Add(new TooltipBrickIconStatValue(item.Name, $"-{component2.Count}", null, null, TooltipBrickIconStatValueType.Negative));
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

	private void AddModifiers(List<ITooltipBrick> bricks, string title, IEnumerable<ProfitFactorModifierVM> mods, bool isPositive)
	{
		if (!mods.Any())
		{
			return;
		}
		bricks.Add(new TooltipBricksGroupStart());
		bricks.Add(new TooltipBrickTitle(title, TooltipTitleType.H4));
		foreach (ProfitFactorModifierVM mod in mods)
		{
			AddModifier(bricks, mod, isPositive);
		}
		bricks.Add(new TooltipBricksGroupEnd());
	}

	private void AddModifier(List<ITooltipBrick> bricks, ProfitFactorModifierVM mod, bool isPositive)
	{
		bricks.Add(GetModBrick(mod, isPositive));
	}

	public static TooltipBrickIconStatValue GetModBrick(ProfitFactorModifierVM mod, bool isPositive)
	{
		string value = mod.ModifierValue.Value.ToString("+0.#;-0.#");
		TooltipBrickIconStatValueType type = (isPositive ? TooltipBrickIconStatValueType.Positive : TooltipBrickIconStatValueType.Negative);
		return new TooltipBrickIconStatValue(GetModifierName(mod), value, null, null, type);
	}

	private static string GetModifierName(ProfitFactorModifierVM mod)
	{
		switch (mod.Type)
		{
		case ProfitFactorModifierType.Project:
		{
			BlueprintColonyProject blueprintColonyProject = mod.Modifier.Modifier as BlueprintColonyProject;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColonyProject?.Name);
		}
		case ProfitFactorModifierType.Event:
		{
			BlueprintColonyEvent blueprintColonyEvent = mod.Modifier.Modifier as BlueprintColonyEvent;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColonyEvent?.Name);
		}
		case ProfitFactorModifierType.Order:
		{
			BlueprintQuestContract blueprintQuestContract = mod.Modifier.Modifier as BlueprintQuestContract;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintQuestContract?.Name);
		}
		case ProfitFactorModifierType.Chronicles:
		{
			BlueprintColonyChronicle blueprintColonyChronicle = mod.Modifier.Modifier as BlueprintColonyChronicle;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColonyChronicle?.Name);
		}
		case ProfitFactorModifierType.ResourceShortage:
		{
			BlueprintResource blueprintResource = mod.Modifier.Modifier as BlueprintResource;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintResource?.Name);
		}
		case ProfitFactorModifierType.ColonyFoundation:
		{
			BlueprintColony blueprintColony = mod.Modifier.Modifier as BlueprintColony;
			return string.Format(UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type), blueprintColony?.Name);
		}
		case ProfitFactorModifierType.Answer:
		{
			BlueprintAnswer blueprintAnswer = mod.Modifier.Modifier as BlueprintAnswer;
			return string.IsNullOrWhiteSpace(blueprintAnswer?.Description) ? UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type) : blueprintAnswer?.Description;
		}
		case ProfitFactorModifierType.Cue:
		{
			BlueprintCue blueprintCue = mod.Modifier.Modifier as BlueprintCue;
			return string.IsNullOrWhiteSpace(blueprintCue?.Description) ? UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type) : blueprintCue?.Description;
		}
		case ProfitFactorModifierType.Other:
		case ProfitFactorModifierType.Companion:
		case ProfitFactorModifierType.Respec:
			return UIStrings.Instance.ProfitFactorTexts.GetSource(mod.Type);
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
