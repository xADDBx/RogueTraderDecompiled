using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("85942dabc27a46958eda38ecb735761d")]
public class RecalculateColonyResources : GameAction
{
	public override string GetCaption()
	{
		return "For player upgrader only!";
	}

	public override void RunAction()
	{
		ProfitFactor profitFactor = Game.Instance.Player.ProfitFactor;
		foreach (ProfitFactorModifier item in profitFactor.GetModifiersByType(ProfitFactorModifierType.ResourceShortage))
		{
			profitFactor.RemoveModifier(item.Modifier);
		}
		ColoniesState coloniesState = Game.Instance.Player.ColoniesState;
		coloniesState.ResourcesNotFromColonies.Clear();
		foreach (ColoniesState.MinerData miner in coloniesState.Miners)
		{
			BlueprintResource resource = miner.Resource;
			int resourceFromMinerCountWithProductivity = ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(miner);
			AddNotFromColony(resource, resourceFromMinerCountWithProductivity);
		}
		IEnumerable<Contract> enumerable = from q in Game.Instance.Player.QuestBook.Quests
			where q is Contract && q.State == QuestState.Completed
			select q as Contract;
		foreach (Contract item2 in enumerable)
		{
			item2?.ResourceShortage.Clear();
			foreach (RewardResourceNotFromColony item3 in (item2?.Blueprint.GetComponents<RewardResourceNotFromColony>())?.EmptyIfNull())
			{
				BlueprintResource resource2 = item3.Resource;
				int count = item3.Count;
				AddNotFromColony(resource2, count);
			}
		}
		foreach (BlueprintCueBase shownCue in Game.Instance.Player.Dialog.ShownCues)
		{
			foreach (RewardResourceNotFromColony item4 in shownCue.GetComponents<RewardResourceNotFromColony>()?.EmptyIfNull())
			{
				BlueprintResource resource3 = item4.Resource;
				int count2 = item4.Count;
				AddNotFromColony(resource3, count2);
			}
			if (!(shownCue is BlueprintCue blueprintCue) || !blueprintCue.OnStop.HasActions)
			{
				continue;
			}
			GameAction[] array = blueprintCue.OnStop?.Actions;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is GainColonyResources { Resources: var resources })
				{
					foreach (ResourceData obj in resources)
					{
						BlueprintResourceReference resource4 = obj.Resource;
						int count3 = obj.Count;
						AddNotFromColony(resource4, count3);
					}
				}
			}
		}
		foreach (BlueprintAnswer selectedAnswer in Game.Instance.Player.Dialog.SelectedAnswers)
		{
			foreach (RewardResourceNotFromColony item5 in selectedAnswer.GetComponents<RewardResourceNotFromColony>()?.EmptyIfNull())
			{
				BlueprintResource resource5 = item5.Resource;
				int count4 = item5.Count;
				AddNotFromColony(resource5, count4);
			}
			if (!selectedAnswer.OnSelect.HasActions)
			{
				continue;
			}
			GameAction[] array = selectedAnswer.OnSelect.Actions;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] is GainColonyResources { Resources: var resources2 })
				{
					foreach (ResourceData obj2 in resources2)
					{
						BlueprintResourceReference resource6 = obj2.Resource;
						int count5 = obj2.Count;
						AddNotFromColony(resource6, count5);
					}
				}
			}
		}
		foreach (ColoniesState.ColonyData colony4 in coloniesState.Colonies)
		{
			Colony colony = colony4.Colony;
			colony.AvailableProducedResources.Clear();
			colony.InitialResources.Clear();
			ResourceData[] resourcesProducedFromStart = colony.Blueprint.ResourcesProducedFromStart;
			foreach (ResourceData resourceData in resourcesProducedFromStart)
			{
				colony.InitialResources.Add(resourceData.Resource, resourceData.Count);
				colony.AvailableProducedResources.Add(resourceData.Resource, resourceData.Count);
			}
		}
		foreach (ColoniesState.ColonyData colony5 in coloniesState.Colonies)
		{
			Colony colony2 = colony5.Colony;
			foreach (ColonyProject project in colony2.Projects)
			{
				if (!project.IsFinished)
				{
					continue;
				}
				project.ProducedResourcesWithoutModifiers.Clear();
				foreach (RewardResourceProject item6 in project.Blueprint.GetComponents<RewardResourceProject>()?.EmptyIfNull())
				{
					BlueprintResource resource7 = item6.Resource;
					project.ProducedResourcesWithoutModifiers.Add(item6.Resource, item6.Count);
					int producedResourceCountWithEfficiencyModifier = ColoniesStateHelper.GetProducedResourceCountWithEfficiencyModifier(item6.Count, colony2.Efficiency.Value);
					AddToColony(colony2, resource7, producedResourceCountWithEfficiencyModifier);
				}
			}
		}
		coloniesState.OrdersUseResources.Clear();
		foreach (Contract item7 in enumerable)
		{
			foreach (RequirementResourceUseOrder item8 in (item7?.Blueprint.GetComponents<RequirementResourceUseOrder>())?.EmptyIfNull())
			{
				Game.Instance.ColonizationController.UseResourceFromPool(item8.ResourceBlueprint, item8.Count, needEvent: false);
			}
		}
		foreach (BlueprintCueBase shownCue2 in Game.Instance.Player.Dialog.ShownCues)
		{
			foreach (RequirementResourceUseDialog item9 in shownCue2.GetComponents<RequirementResourceUseDialog>()?.EmptyIfNull())
			{
				Game.Instance.ColonizationController.UseResourceFromPool(item9.ResourceBlueprint, item9.Count, needEvent: false);
			}
		}
		foreach (BlueprintAnswer selectedAnswer2 in Game.Instance.Player.Dialog.SelectedAnswers)
		{
			foreach (RequirementResourceUseDialog item10 in selectedAnswer2.GetComponents<RequirementResourceUseDialog>()?.EmptyIfNull())
			{
				Game.Instance.ColonizationController.UseResourceFromPool(item10.ResourceBlueprint, item10.Count, needEvent: false);
			}
		}
		foreach (ColoniesState.ColonyData colony6 in coloniesState.Colonies)
		{
			Colony colony3 = colony6.Colony;
			foreach (ColonyProject project2 in colony3.Projects)
			{
				project2.UsedResourcesFromPool.Clear();
				project2.ResourceShortage.Clear();
			}
			foreach (ColonyProject project3 in colony3.Projects)
			{
				foreach (RequirementResourceUseProject item11 in project3.Blueprint.GetComponents<RequirementResourceUseProject>()?.EmptyIfNull())
				{
					BlueprintResource resourceBlueprint = item11.ResourceBlueprint;
					int count6 = item11.Count;
					Game.Instance.ColonizationController.UseResourceFromPool(resourceBlueprint, count6, needEvent: false);
					project3.UsedResourcesFromPool.Add(resourceBlueprint, count6);
				}
			}
		}
	}

	private void AddNotFromColony(BlueprintResource resource, int count)
	{
		Dictionary<BlueprintResource, int> resourcesNotFromColonies = Game.Instance.Player.ColoniesState.ResourcesNotFromColonies;
		if (resourcesNotFromColonies.ContainsKey(resource))
		{
			resourcesNotFromColonies[resource] += count;
		}
		else
		{
			resourcesNotFromColonies.Add(resource, count);
		}
	}

	private void AddToColony(Colony colony, BlueprintResource resource, int count)
	{
		if (colony.AvailableProducedResources.ContainsKey(resource))
		{
			colony.AvailableProducedResources[resource] += count;
		}
		else
		{
			colony.AvailableProducedResources.Add(resource, count);
		}
	}
}
