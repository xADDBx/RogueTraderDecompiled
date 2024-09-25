using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Controllers.GlobalMap;

public class ColonizationController : IControllerEnable, IController, IControllerTick
{
	public bool NeedToOpenExplorationScreen;

	public PlanetEntity LastColonizedPlanet;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			colony.Colony.Tick();
		}
		if (NeedToOpenExplorationScreen)
		{
			EventBus.RaiseEvent(delegate(IOpenExplorationScreenAfterColonization h)
			{
				h.HandleTryOpenExplorationScreenAfterColonization(LastColonizedPlanet);
			});
			NeedToOpenExplorationScreen = false;
		}
	}

	private bool CanColonize(PlanetEntity planet)
	{
		if (planet.IsColonized)
		{
			PFLog.Default.Warning("Trying to colonize already colonized planet");
			return false;
		}
		if (!planet.IsScanned)
		{
			PFLog.Default.Warning("Trying to colonize not scanned planet");
			return false;
		}
		if (planet.Colony != null)
		{
			PFLog.Default.Warning("Planet already colonized");
			return false;
		}
		if (Game.Instance.Player.ProfitFactor.Total < BlueprintWarhammerRoot.Instance.ColonyRoot.ColonizationCostInPF)
		{
			PFLog.Default.Warning("Don't have enough profit factor");
			return false;
		}
		return true;
	}

	public void Colonize(PlanetEntity planet, bool isPlayerCommand = true)
	{
		if (CanColonize(planet))
		{
			List<BlueprintColonyEvent> events = ColoniesGenerator.RandomizeEventsForColony(planet.Blueprint);
			Game.Instance.GameCommandQueue.CreateColony(planet, events, isPlayerCommand);
		}
	}

	[CanBeNull]
	public Colony GetColony(SectorMapObject sectorMapObject)
	{
		if (!sectorMapObject.IsSystem)
		{
			return null;
		}
		BlueprintStarSystemMap area = sectorMapObject.StarSystemBlueprint.StarSystemToTransit?.Get() as BlueprintStarSystemMap;
		return GetColonyWithBlueprint(area);
	}

	public Colony GetColonyWithBlueprint(BlueprintStarSystemMap area)
	{
		if (area != null)
		{
			return Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Area == area)?.Colony;
		}
		return null;
	}

	public Dictionary<BlueprintResource, int> AllShortages()
	{
		Dictionary<BlueprintResource, int> dictionary = new Dictionary<BlueprintResource, int>();
		BlueprintResource key;
		int value;
		foreach (KeyValuePair<BlueprintResource, int> item in Game.Instance.Player.ColoniesState.GlobalResourceShortage)
		{
			item.Deconstruct(out key, out value);
			BlueprintResource blueprintResource = key;
			int num = value;
			if (dictionary.ContainsKey(blueprintResource))
			{
				Dictionary<BlueprintResource, int> dictionary2 = dictionary;
				key = blueprintResource;
				dictionary2[key] += num;
			}
			else
			{
				dictionary.Add(blueprintResource, num);
			}
		}
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			foreach (ColonyProject project in colony.Colony.Projects)
			{
				foreach (KeyValuePair<BlueprintResource, int> item2 in project.ResourceShortage)
				{
					item2.Deconstruct(out key, out value);
					BlueprintResource blueprintResource2 = key;
					int num2 = value;
					if (dictionary.ContainsKey(blueprintResource2))
					{
						Dictionary<BlueprintResource, int> dictionary2 = dictionary;
						key = blueprintResource2;
						dictionary2[key] += num2;
					}
					else
					{
						dictionary.Add(blueprintResource2, num2);
					}
				}
			}
		}
		foreach (BlueprintQuestContract orderBp in Game.Instance.Player.ColoniesState.OrdersUseResources)
		{
			if (!(Game.Instance.Player.QuestBook.Quests.FirstOrDefault((Quest quest) => quest.Blueprint == orderBp) is Contract contract))
			{
				continue;
			}
			foreach (KeyValuePair<BlueprintResource, int> item3 in contract.ResourceShortage)
			{
				item3.Deconstruct(out key, out value);
				BlueprintResource blueprintResource3 = key;
				int num3 = value;
				if (dictionary.ContainsKey(blueprintResource3))
				{
					Dictionary<BlueprintResource, int> dictionary2 = dictionary;
					key = blueprintResource3;
					dictionary2[key] += num3;
				}
				else
				{
					dictionary.Add(blueprintResource3, num3);
				}
			}
		}
		return dictionary;
	}

	public Dictionary<BlueprintResource, int> AllResourcesInPool()
	{
		Dictionary<BlueprintResource, int> dictionary = new Dictionary<BlueprintResource, int>();
		BlueprintResource key;
		int value;
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			foreach (KeyValuePair<BlueprintResource, int> availableProducedResource in colony.Colony.AvailableProducedResources)
			{
				availableProducedResource.Deconstruct(out key, out value);
				BlueprintResource blueprintResource = key;
				int num = value;
				if (dictionary.ContainsKey(blueprintResource))
				{
					Dictionary<BlueprintResource, int> dictionary2 = dictionary;
					key = blueprintResource;
					dictionary2[key] += num;
				}
				else
				{
					dictionary.Add(blueprintResource, num);
				}
			}
		}
		foreach (KeyValuePair<BlueprintResource, int> resourcesNotFromColony in Game.Instance.Player.ColoniesState.ResourcesNotFromColonies)
		{
			resourcesNotFromColony.Deconstruct(out key, out value);
			BlueprintResource blueprintResource2 = key;
			int num2 = value;
			if (dictionary.ContainsKey(blueprintResource2))
			{
				Dictionary<BlueprintResource, int> dictionary2 = dictionary;
				key = blueprintResource2;
				dictionary2[key] += num2;
			}
			else
			{
				dictionary.Add(blueprintResource2, num2);
			}
		}
		return dictionary;
	}

	public void UseResourceFromPool(BlueprintResource resource, int resourceCount, bool needEvent = true)
	{
		ColoniesState coloniesState = Game.Instance.Player.ColoniesState;
		int initialCount = resourceCount;
		if (coloniesState.ResourcesNotFromColonies.TryGetValue(resource, out var value))
		{
			int minCount = Math.Min(value, resourceCount);
			coloniesState.ResourcesNotFromColonies[resource] -= minCount;
			resourceCount -= minCount;
			if (needEvent)
			{
				EventBus.RaiseEvent(delegate(IColonizationResourcesHandler h)
				{
					h.HandleNotFromColonyResourcesUpdated(resource, minCount);
				});
			}
		}
		if (resourceCount == 0)
		{
			return;
		}
		foreach (ColoniesState.ColonyData colony in coloniesState.Colonies)
		{
			resourceCount = colony.Colony.UseResourceFromColony(resource, resourceCount);
			if (resourceCount == 0)
			{
				break;
			}
		}
		if (needEvent)
		{
			EventBus.RaiseEvent(delegate(IColonizationResourcesHandler h)
			{
				h.HandleColonyResourcesUpdated(resource, initialCount - resourceCount);
			});
		}
		if (resourceCount > 0)
		{
			AddResourceShortage(resource, resourceCount);
		}
	}

	private int AddResourceShortageInProject(BlueprintResource resource, int resourceCount)
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			foreach (ColonyProject project in colony.Colony.Projects)
			{
				if (project.UsedResourcesFromPool.TryGetValue(resource, out var value))
				{
					int num = Mathf.Min(resourceCount, value);
					if (project.ResourceShortage.ContainsKey(resource))
					{
						project.ResourceShortage[resource] += num;
					}
					else
					{
						project.ResourceShortage.Add(resource, num);
					}
					resourceCount -= num;
					if (resourceCount == 0)
					{
						return resourceCount;
					}
				}
			}
		}
		return resourceCount;
	}

	private void AddResourceShortageGlobal(BlueprintResource resource, int resourceCount)
	{
		if (resourceCount > 0)
		{
			Dictionary<BlueprintResource, int> globalResourceShortage = Game.Instance.Player.ColoniesState.GlobalResourceShortage;
			if (globalResourceShortage.ContainsKey(resource))
			{
				globalResourceShortage[resource] += resourceCount;
			}
			else
			{
				globalResourceShortage.Add(resource, resourceCount);
			}
		}
	}

	private int AddResourceShortageInOrder(BlueprintResource resource, int resourceCount)
	{
		foreach (BlueprintQuestContract orderBp in Game.Instance.Player.ColoniesState.OrdersUseResources)
		{
			RequirementResourceUseOrder requirementResourceUseOrder = orderBp.GetComponents<RequirementResourceUseOrder>().EmptyIfNull().FirstOrDefault((RequirementResourceUseOrder requirement) => requirement.ResourceBlueprint == resource);
			if (requirementResourceUseOrder?.ResourceBlueprint == null)
			{
				continue;
			}
			int num = Mathf.Min(resourceCount, requirementResourceUseOrder.Count);
			if (!(Game.Instance.Player.QuestBook.Quests.FirstOrDefault((Quest quest) => quest.Blueprint == orderBp) is Contract contract))
			{
				continue;
			}
			if (num > 0)
			{
				if (contract.ResourceShortage.ContainsKey(resource))
				{
					contract.ResourceShortage[resource] += num;
				}
				else
				{
					contract.ResourceShortage.Add(resource, num);
				}
			}
			resourceCount -= num;
			if (resourceCount == 0)
			{
				return resourceCount;
			}
		}
		return resourceCount;
	}

	private int RemoveResourceShortageInOrder(BlueprintResource resource, int resourceCount)
	{
		foreach (BlueprintQuestContract orderBp in Game.Instance.Player.ColoniesState.OrdersUseResources)
		{
			if (Game.Instance.Player.QuestBook.Quests.FirstOrDefault((Quest quest) => quest.Blueprint == orderBp) is Contract contract && contract.ResourceShortage.TryGetValue(resource, out var value))
			{
				int num = Mathf.Min(value, resourceCount);
				if (value > resourceCount)
				{
					contract.ResourceShortage[resource] -= resourceCount;
				}
				else
				{
					contract.ResourceShortage.Remove(resource);
				}
				resourceCount -= num;
				if (resourceCount == 0)
				{
					return resourceCount;
				}
			}
		}
		return resourceCount;
	}

	private int RemoveResourceShortageInProject(BlueprintResource resource, int resourceCount)
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			foreach (ColonyProject project in colony.Colony.Projects)
			{
				if (project.ResourceShortage.TryGetValue(resource, out var value))
				{
					int num = Mathf.Min(value, resourceCount);
					if (value > resourceCount)
					{
						project.ResourceShortage[resource] -= resourceCount;
					}
					else
					{
						project.ResourceShortage.Remove(resource);
					}
					resourceCount -= num;
					if (resourceCount == 0)
					{
						return resourceCount;
					}
				}
			}
		}
		return resourceCount;
	}

	private int RemoveResourceShortageGlobal(BlueprintResource resource, int resourceCount)
	{
		if (resourceCount <= 0)
		{
			return resourceCount;
		}
		Dictionary<BlueprintResource, int> globalResourceShortage = Game.Instance.Player.ColoniesState.GlobalResourceShortage;
		if (globalResourceShortage.TryGetValue(resource, out var value))
		{
			int num = Mathf.Min(value, resourceCount);
			if (num > value)
			{
				globalResourceShortage[resource] -= num;
			}
			else
			{
				globalResourceShortage.Remove(resource);
			}
			resourceCount -= num;
		}
		return resourceCount;
	}

	public void AddResourceShortage(BlueprintResource resource, int resourceCount)
	{
		int num = resourceCount;
		if (resourceCount > 0)
		{
			resourceCount = AddResourceShortageInProject(resource, resourceCount);
		}
		if (resourceCount > 0)
		{
			resourceCount = AddResourceShortageInOrder(resource, resourceCount);
		}
		if (resourceCount > 0)
		{
			AddResourceShortageGlobal(resource, resourceCount);
			resourceCount = 0;
		}
		float num2 = resource.ProfitFactorPenalty * (float)num;
		ProfitFactor profitFactor = Game.Instance.Player.ProfitFactor;
		float? modifierValue = profitFactor.GetModifierValue(resource);
		if (modifierValue.HasValue)
		{
			profitFactor.RemoveModifier(resource);
			profitFactor.AddModifier(modifierValue.Value - num2, ProfitFactorModifierType.ResourceShortage, resource);
		}
		else
		{
			profitFactor.AddModifier(0f - num2, ProfitFactorModifierType.ResourceShortage, resource);
		}
	}

	public int RemoveResourceShortage([CanBeNull] Colony colony, BlueprintResource resource, int resourceCount)
	{
		int num = resourceCount;
		if (resourceCount > 0)
		{
			resourceCount = RemoveResourceShortageInOrder(resource, resourceCount);
		}
		if (resourceCount > 0)
		{
			resourceCount = RemoveResourceShortageInProject(resource, resourceCount);
		}
		if (resourceCount > 0)
		{
			resourceCount = RemoveResourceShortageGlobal(resource, resourceCount);
		}
		if (num - resourceCount > 0)
		{
			colony?.UseResourceFromColony(resource, num - resourceCount);
		}
		float num2 = resource.ProfitFactorPenalty * (float)(num - resourceCount);
		ProfitFactor profitFactor = Game.Instance.Player.ProfitFactor;
		float? modifierValue = profitFactor.GetModifierValue(resource);
		if (modifierValue.HasValue)
		{
			profitFactor.RemoveModifier(resource);
			profitFactor.AddModifier(modifierValue.Value + num2, ProfitFactorModifierType.ResourceShortage, resource);
		}
		else
		{
			profitFactor.AddModifier(num2, ProfitFactorModifierType.ResourceShortage, resource);
		}
		return resourceCount;
	}

	public void OnEnable()
	{
		Game.Instance.Player.ColoniesState.Initialize();
	}

	public void ChangeMinerProductivity(int productivityModificator, BlueprintScriptableObject modifier, ColonyStatModifierType type)
	{
		if (productivityModificator == 0)
		{
			return;
		}
		ColonyStat minerProductivity = Game.Instance.Player.ColoniesState.MinerProductivity;
		int value = minerProductivity.Value;
		minerProductivity.Modifiers.Add(new ColonyStatModifier
		{
			Value = productivityModificator,
			Modifier = modifier,
			ModifierType = type
		});
		int value2 = minerProductivity.Value;
		foreach (ColoniesState.MinerData miner in Game.Instance.Player.ColoniesState.Miners)
		{
			int resourceFromMinerCountWithProductivity = ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(miner, value);
			int resourceFromMinerCountWithProductivity2 = ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(miner, value2);
			if (value < value2)
			{
				AddResourceNotFromColonyToPool(miner.Resource, resourceFromMinerCountWithProductivity2 - resourceFromMinerCountWithProductivity);
			}
			else if (value > value2)
			{
				RemoveResourceNotFromColony(miner.Resource, resourceFromMinerCountWithProductivity - resourceFromMinerCountWithProductivity2);
			}
		}
	}

	public int ResourceMinersCount()
	{
		return Game.Instance.Player.Inventory.Items.Where((ItemEntity item) => item.Blueprint.ItemType == ItemsItemType.ResourceMiner).Sum((ItemEntity extractorItem) => extractorItem.Count);
	}

	public bool HaveResourceMiner()
	{
		return Game.Instance.Player.Inventory.Items.Contains((ItemEntity item) => item.Blueprint.ItemType == ItemsItemType.ResourceMiner);
	}

	public void UseResourceMiner(StarSystemObjectEntity sso, BlueprintResource resource)
	{
		Game.Instance.GameCommandQueue.UseResourceMiner(sso.Blueprint, resource);
	}

	public void RemoveResourceMiner(StarSystemObjectEntity sso, BlueprintResource resource)
	{
		Game.Instance.GameCommandQueue.RemoveResourceMiner(sso.Blueprint, resource);
	}

	public void AddResourceFromMiner(ColoniesState.MinerData minerData, BlueprintResource resource)
	{
		int resourceFromMinerCountWithProductivity = ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(minerData);
		AddResourceNotFromColonyToPool(resource, resourceFromMinerCountWithProductivity);
	}

	public void AddResourceNotFromColonyToPool(BlueprintResource resource, int count)
	{
		if (count <= 0)
		{
			return;
		}
		int num = RemoveResourceShortage(null, resource, count);
		if (num > 0)
		{
			Dictionary<BlueprintResource, int> resourcesNotFromColonies = Game.Instance.Player.ColoniesState.ResourcesNotFromColonies;
			if (resourcesNotFromColonies.ContainsKey(resource))
			{
				resourcesNotFromColonies[resource] += num;
			}
			else
			{
				resourcesNotFromColonies.Add(resource, num);
			}
		}
		EventBus.RaiseEvent(delegate(IColonizationResourcesHandler h)
		{
			h.HandleColonyResourcesUpdated(resource, count);
		});
	}

	public void RemoveResourceNotFromColony(BlueprintResource resource, int count)
	{
		if (count <= 0)
		{
			return;
		}
		Dictionary<BlueprintResource, int> resourcesNotFromColonies = Game.Instance.Player.ColoniesState.ResourcesNotFromColonies;
		if (resourcesNotFromColonies.TryGetValue(resource, out var value))
		{
			if (value - count > 0)
			{
				resourcesNotFromColonies[resource] -= count;
			}
			else if (value - count == 0)
			{
				resourcesNotFromColonies.Remove(resource);
			}
			else
			{
				resourcesNotFromColonies.Remove(resource);
				AddResourceShortage(resource, count - value);
			}
		}
		else
		{
			AddResourceShortage(resource, count);
		}
		EventBus.RaiseEvent(delegate(IColonizationResourcesHandler h)
		{
			h.HandleColonyResourcesUpdated(resource, count);
		});
	}

	public bool CheckOnColony(BlueprintColony colony)
	{
		if (!(Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap))
		{
			return false;
		}
		if (colony == null)
		{
			return false;
		}
		StarSystemObjectView starSystemObjectLandOn = Game.Instance.StarSystemMapController.StarSystemShip.StarSystemObjectLandOn;
		ColoniesState.ColonyData colonyData = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == colony);
		if (starSystemObjectLandOn == null || colonyData == null)
		{
			return false;
		}
		return colonyData.Planet == starSystemObjectLandOn.Data.Blueprint;
	}
}
