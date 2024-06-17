using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI.MVVM.VM.SystemMapNotification;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class Colony : IHashable
{
	public static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Colony");

	[JsonProperty]
	public List<ColonyProject> Projects = new List<ColonyProject>();

	[JsonProperty]
	public BlueprintPlanet Planet;

	[JsonProperty]
	public BlueprintColony Blueprint;

	[JsonProperty]
	public TimeSpan ColonyFoundationTime;

	[JsonProperty]
	public ColonyStat Contentment;

	[JsonProperty]
	public ColonyStat Efficiency;

	[JsonProperty]
	public ColonyStat Security;

	[JsonProperty]
	public Dictionary<BlueprintResource, int> AvailableProducedResources = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public Dictionary<BlueprintResource, int> InitialResources = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public Dictionary<BlueprintResource, int> UsedProducedResources = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public Dictionary<BlueprintColonyTrait, TimeSpan> ColonyTraits = new Dictionary<BlueprintColonyTrait, TimeSpan>();

	[JsonProperty]
	private BlueprintColonyEventsRoot.ColonyEventToTimer m_CurrentEvent;

	[JsonProperty]
	public List<BlueprintColonyEventsRoot.ColonyEventToTimer> AllEventsForColony = new List<BlueprintColonyEventsRoot.ColonyEventToTimer>();

	[JsonProperty]
	private List<BlueprintColonyEventsRoot.ColonyEventToTimer> m_NotStartedEvents = new List<BlueprintColonyEventsRoot.ColonyEventToTimer>();

	[JsonProperty]
	public List<BlueprintColonyEvent> StartedEvents = new List<BlueprintColonyEvent>();

	[JsonProperty]
	private TimeSpan m_LastEventTime;

	[JsonProperty]
	public List<BlueprintDialog> StartedChronicles = new List<BlueprintDialog>();

	[JsonProperty]
	public List<ColonyChronicle> Chronicles = new List<ColonyChronicle>();

	[JsonProperty]
	public ColonyLootHolder LootToReceive;

	[JsonProperty]
	public List<Consumable> Consumables = new List<Consumable>();

	[JsonProperty]
	public List<ColonyProject> FinishedProjectsSinceLastVisit = new List<ColonyProject>();

	[MemoryPackConstructor]
	public Colony(JsonConstructorMark _)
	{
	}

	public Colony(PlanetEntity planet, List<BlueprintColonyEventsRoot.ColonyEventToTimer> events)
	{
		Blueprint = planet.Blueprint.GetComponent<ColonyComponent>()?.ColonyBlueprint?.Get();
		if (Blueprint != null)
		{
			Planet = planet.Blueprint;
			planet.IsColonized = true;
			SetupColony(planet, events);
		}
	}

	public void PostLoad()
	{
		LootToReceive?.Items?.PrePostLoad();
		LootToReceive?.Items?.PostLoad();
	}

	private void SetupColony(PlanetEntity planet, List<BlueprintColonyEventsRoot.ColonyEventToTimer> events)
	{
		ColonyFoundationTime = Game.Instance.TimeController.GameTime;
		ResourceData[] array = Blueprint.ResourcesProducedFromStart.EmptyIfNull();
		foreach (ResourceData resourceData in array)
		{
			AddResourceFromColony(resourceData.Resource, resourceData.Count);
			InitialResources.Add(resourceData.Resource, resourceData.Count);
		}
		Efficiency = new ColonyStat
		{
			InitialValue = Blueprint.InitialEfficiency,
			MinValue = BlueprintWarhammerRoot.Instance.ColonyRoot.MinEfficiency,
			MaxValue = BlueprintWarhammerRoot.Instance.ColonyRoot.MaxEfficiency
		};
		Contentment = new ColonyStat
		{
			InitialValue = Blueprint.InitialContentment,
			MinValue = BlueprintWarhammerRoot.Instance.ColonyRoot.MinContentment,
			MaxValue = BlueprintWarhammerRoot.Instance.ColonyRoot.MaxContentment
		};
		Security = new ColonyStat
		{
			InitialValue = Blueprint.InitialSecurity,
			MinValue = BlueprintWarhammerRoot.Instance.ColonyRoot.MinSecurity,
			MaxValue = BlueprintWarhammerRoot.Instance.ColonyRoot.MaxSecurity
		};
		if (BlueprintWarhammerRoot.Instance.ColonyRoot.ColonizationCostInPF > 0f)
		{
			Game.Instance.Player.ProfitFactor.AddModifier(0f - BlueprintWarhammerRoot.Instance.ColonyRoot.ColonizationCostInPF, ProfitFactorModifierType.ColonyFoundation, Blueprint);
		}
		Game.Instance.Player.ColoniesState.Colonies.Add(new ColoniesState.ColonyData
		{
			Area = (Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap),
			Planet = planet.Blueprint,
			Colony = this
		});
		BlueprintColonyTrait.Reference[] traits = Blueprint.Traits;
		foreach (BlueprintColonyTrait.Reference reference in traits)
		{
			AddTrait(reference);
		}
		foreach (BlueprintColonyTrait trait in planet.Traits)
		{
			AddTrait(trait);
		}
		foreach (BlueprintColonyTrait traitsForAllColony in Game.Instance.Player.ColoniesState.TraitsForAllColonies)
		{
			AddTrait(traitsForAllColony);
		}
		foreach (ColonyStatModifier contentmentModifiersForAllColony in Game.Instance.Player.ColoniesState.ContentmentModifiersForAllColonies)
		{
			Contentment.Modifiers.Add(contentmentModifiersForAllColony);
		}
		foreach (ColonyStatModifier securityModifiersForAllColony in Game.Instance.Player.ColoniesState.SecurityModifiersForAllColonies)
		{
			Security.Modifiers.Add(securityModifiersForAllColony);
		}
		foreach (ColonyStatModifier efficiencyModifiersForAllColony in Game.Instance.Player.ColoniesState.EfficiencyModifiersForAllColonies)
		{
			Efficiency.Modifiers.Add(efficiencyModifiersForAllColony);
		}
		AllEventsForColony = events;
		m_NotStartedEvents = AllEventsForColony.ToList();
		m_LastEventTime = Game.Instance.TimeController.GameTime;
		m_CurrentEvent = (m_NotStartedEvents.Empty() ? null : m_NotStartedEvents[0]);
		LootToReceive = new ColonyLootHolder(this);
		EventBus.RaiseEvent(delegate(IColonizationHandler h)
		{
			h.HandleColonyCreated(this, planet);
		});
	}

	public ColonyEventState GetEventState(BlueprintColonyEvent colonyEvent)
	{
		if (AllEventsForColony.All((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent != colonyEvent))
		{
			return ColonyEventState.None;
		}
		if (StartedEvents.Contains(colonyEvent))
		{
			return ColonyEventState.Started;
		}
		if (!m_NotStartedEvents.Any((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent == colonyEvent))
		{
			return ColonyEventState.Finished;
		}
		return ColonyEventState.Scheduled;
	}

	public void RemoveEvent(BlueprintColonyEvent colonyEvent)
	{
		if (AllEventsForColony.All((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent != colonyEvent))
		{
			Logger.Error($"Can not remove event {colonyEvent} from colony {Planet} : event not in list");
			return;
		}
		AllEventsForColony.Remove((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent == colonyEvent);
		m_NotStartedEvents.Remove((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent == colonyEvent);
		StartedEvents.Remove((BlueprintColonyEvent x) => x == colonyEvent);
		if (m_CurrentEvent.ColonyEvent == colonyEvent)
		{
			m_CurrentEvent = (m_NotStartedEvents.Empty() ? null : m_NotStartedEvents[0]);
		}
		Logger.Log($"Event {colonyEvent} removed from colony {Planet}");
	}

	public void AddEvent(BlueprintColonyEvent colonyEvent, bool startImmediately = false)
	{
		if (!colonyEvent.IsAllowedOnPlanet(Planet))
		{
			Logger.Error($"Can not add colony event {colonyEvent} to {Planet}: not allowed on this colony");
			return;
		}
		BlueprintColonyEventsRoot.ColonyEventToTimer colonyEventToTimer = BlueprintWarhammerRoot.Instance.ColonyRoot.ColonyEvents.Get().Events.FirstOrDefault((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent == colonyEvent);
		if (colonyEventToTimer == null)
		{
			Logger.Error($"Can not add event {colonyEvent} to colony {Planet} : no timer data in events root");
			return;
		}
		if (AllEventsForColony.Any((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent == colonyEvent))
		{
			Logger.Error($"Can not add event {colonyEvent} to colony {Planet} : already in list");
			return;
		}
		AllEventsForColony.Add(colonyEventToTimer);
		if (startImmediately)
		{
			StartedEvents.Add(colonyEventToTimer.ColonyEvent);
		}
		else
		{
			m_NotStartedEvents.Add(colonyEventToTimer);
		}
		if (m_CurrentEvent == null)
		{
			m_CurrentEvent = (m_NotStartedEvents.Empty() ? null : m_NotStartedEvents[0]);
		}
		Logger.Log($"Event {colonyEvent} added to colony {Planet}. StartImmediately={startImmediately}");
	}

	public void AddTrait(BlueprintColonyTrait trait)
	{
		if (trait != null && !ColonyTraits.ContainsKey(trait))
		{
			ColonyTraits.Add(trait, Game.Instance.TimeController.GameTime);
			ChangeContentment(trait.ContentmentModifier, ColonyStatModifierType.Trait, trait);
			ChangeEfficiency(trait.EfficiencyModifier, ColonyStatModifierType.Trait, trait);
			ChangeSecurity(trait.SecurityModifier, ColonyStatModifierType.Trait, trait);
			EventBus.RaiseEvent(delegate(IColonizationTraitHandler h)
			{
				h.HandleTraitStarted(this, trait);
			});
		}
	}

	public void RemoveTrait(BlueprintColonyTrait trait)
	{
		if (trait != null && ColonyTraits.ContainsKey(trait))
		{
			Contentment.Modifiers.Remove((ColonyStatModifier modifier) => modifier.Modifier == trait);
			Efficiency.Modifiers.Remove((ColonyStatModifier modifier) => modifier.Modifier == trait);
			Security.Modifiers.Remove((ColonyStatModifier modifier) => modifier.Modifier == trait);
			ColonyTraits.Remove(trait);
			EventBus.RaiseEvent(delegate(IColonizationTraitHandler h)
			{
				h.HandleTraitEnded(this, trait);
			});
		}
	}

	public void Tick()
	{
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		foreach (ColonyProject project in Projects)
		{
			if (!project.IsFinished && SegmentsToBuildProject(project.Blueprint) <= (gameTime - project.StartTime).TotalSegments())
			{
				FinishProject(project);
			}
		}
		List<BlueprintColonyTrait> list = new List<BlueprintColonyTrait>();
		foreach (var (blueprintColonyTrait2, timeSpan2) in ColonyTraits)
		{
			if (!blueprintColonyTrait2.IsPermanent && blueprintColonyTrait2.TraitDuration <= (gameTime - timeSpan2).TotalSegments())
			{
				list.Add(blueprintColonyTrait2);
			}
		}
		foreach (BlueprintColonyTrait item in list)
		{
			RemoveTrait(item);
		}
		if (m_CurrentEvent != null && (gameTime - m_LastEventTime).TotalSegments() >= m_CurrentEvent.Segments)
		{
			m_LastEventTime = Game.Instance.TimeController.GameTime;
			if (m_CurrentEvent.ColonyEvent.CanStart())
			{
				StartedEvents.Add(m_CurrentEvent.ColonyEvent);
				m_NotStartedEvents.Remove(m_CurrentEvent);
				EventBus.RaiseEvent(delegate(IColonizationEventHandler h)
				{
					h.HandleEventStarted(this, m_CurrentEvent.ColonyEvent);
				});
				EventBus.RaiseEvent(delegate(IColonyNotificationUIHandler h)
				{
					h.HandleColonyNotification(Blueprint.Name.Text, ColonyNotificationType.Event);
				});
			}
			else
			{
				m_NotStartedEvents.Remove(m_CurrentEvent);
				m_NotStartedEvents.Add(m_CurrentEvent);
			}
			m_CurrentEvent = (m_NotStartedEvents.Empty() ? null : m_NotStartedEvents[0]);
		}
		foreach (Consumable consumable in Consumables)
		{
			if ((gameTime - consumable.LastRefill).TotalSegments() < consumable.SegmentsToRefill)
			{
				continue;
			}
			int num = 0;
			foreach (ItemEntity item2 in LootToReceive.Items.EmptyIfNull())
			{
				if (item2.Blueprint == consumable.Item)
				{
					num += item2.Count;
				}
			}
			if (num < consumable.MaxCount)
			{
				LootToReceive.AddItem(consumable.Item, consumable.MaxCount - num);
			}
			consumable.LastRefill = Game.Instance.TimeController.GameTime;
		}
	}

	public void StartEvent(BlueprintColonyEvent colonyEvent)
	{
		if (StartedEvents.Contains(colonyEvent))
		{
			DialogData dialogData = DialogController.SetupDialogWithoutTarget(colonyEvent.Event, null);
			dialogData.AddContextData<ColonyContextData>().Setup(this, colonyEvent);
			Game.Instance.DialogController.StartDialog(dialogData);
			if (!colonyEvent.CanBeRepeated)
			{
				FinishEvent(colonyEvent, null);
			}
		}
	}

	public void FinishEvent(BlueprintColonyEvent colonyEvent, [CanBeNull] BlueprintColonyEventResult result)
	{
		StartedEvents.Remove(colonyEvent);
		EventBus.RaiseEvent(delegate(IColonizationEventHandler h)
		{
			h.HandleEventFinished(this, colonyEvent, result);
		});
	}

	public void StartChronicle(BlueprintDialog chronicle)
	{
		DialogData dialogData = DialogController.SetupDialogWithoutTarget(chronicle, null);
		dialogData.AddContextData<ColonyContextData>().Setup(this, null);
		Game.Instance.DialogController.StartDialog(dialogData);
	}

	public void AddStartedChronicle(BlueprintDialog chronicle)
	{
		StartedChronicles.Add(chronicle);
		EventBus.RaiseEvent(delegate(IColonyNotificationUIHandler h)
		{
			h.HandleColonyNotification(Blueprint.Name.Text, ColonyNotificationType.Chronicle);
		});
	}

	public void AddFinishedChronicle(ColonyChronicle chronicle)
	{
		StartedChronicles.Remove(chronicle.Blueprint.BlueprintDialog);
		Chronicles.Add(chronicle);
		EventBus.RaiseEvent(delegate(IColonizationChronicleHandler h)
		{
			h.HandleChronicleFinished(this, chronicle);
		});
	}

	public bool ProjectCanStart(BlueprintColonyProject project)
	{
		if (AllProjectsFinished() && ProjectIsAvailable(project))
		{
			return ProjectMeetRequirements(project);
		}
		return false;
	}

	public bool ProjectIsAvailable(BlueprintColonyProject project)
	{
		if (project.AvailabilityConditions != null)
		{
			return project.AvailabilityConditions.Check();
		}
		return true;
	}

	public bool ProjectIsExcluded(BlueprintColonyProject project)
	{
		IEnumerable<RequirementNotBuiltProjectInColony> enumerable = project.GetComponents<RequirementNotBuiltProjectInColony>().EmptyIfNull();
		if (enumerable.Any())
		{
			foreach (RequirementNotBuiltProjectInColony item in enumerable)
			{
				if (!item.Check(this))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AllProjectsFinished()
	{
		return !Projects.Contains((ColonyProject proj) => !proj.IsFinished);
	}

	public bool ProjectMeetRequirements(BlueprintColonyProject project)
	{
		foreach (Requirement component in project.GetComponents<Requirement>())
		{
			if (!component.Check(this))
			{
				return false;
			}
		}
		return true;
	}

	public void StartProject(BlueprintColonyProject project)
	{
		if (!ProjectCanStart(project))
		{
			Logger.Warning("Can't start project " + project.AssetGuid);
			return;
		}
		ColonyProject newProject = new ColonyProject(project)
		{
			StartTime = Game.Instance.TimeController.GameTime
		};
		Projects.Add(newProject);
		foreach (Requirement component in project.GetComponents<Requirement>())
		{
			component.Apply(this);
		}
		StarSystemObjectEntity sso2 = Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity sso) => sso.Blueprint == Planet);
		using (ContextData<StarSystemContextData>.Request().Setup(sso2, Game.Instance.Player.MainCharacterEntity, Game.Instance.Player.PlayerShip))
		{
			project.ActionsOnStart?.Run();
		}
		EventBus.RaiseEvent(delegate(IColonizationProjectsHandler h)
		{
			h.HandleColonyProjectStarted(this, newProject);
		});
	}

	public void SpendResource(BlueprintColonyProject projectBp, ResourceData resourceData)
	{
		ColonyProject colonyProject = Projects.FirstOrDefault((ColonyProject p) => p.Blueprint == projectBp);
		if (colonyProject == null)
		{
			PFLog.Default.Warning("Cannot find project " + projectBp.Name + " in colony " + Blueprint.name);
			return;
		}
		Game.Instance.ColonizationController.UseResourceFromPool(resourceData.Resource.Get(), resourceData.Count);
		colonyProject.UsedResourcesFromPool.Add(resourceData.Resource.Get(), resourceData.Count);
	}

	public void FinishProject(ColonyProject project)
	{
		project.IsFinished = true;
		foreach (Reward component in project.Blueprint.GetComponents<Reward>())
		{
			component.ReceiveReward(this);
		}
		StarSystemObjectEntity sso2 = Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity sso) => sso.Blueprint == Planet);
		using (ContextData<StarSystemContextData>.Request().Setup(sso2, Game.Instance.Player.MainCharacterEntity, Game.Instance.Player.PlayerShip))
		{
			project.Blueprint.ActionsOnFinish?.Run();
		}
		FinishedProjectsSinceLastVisit.Add(project);
		EventBus.RaiseEvent(delegate(IColonizationProjectsHandler h)
		{
			h.HandleColonyProjectFinished(this, project);
		});
	}

	public void ClearFinishedProjectsSinceLastVisit()
	{
		FinishedProjectsSinceLastVisit.Clear();
	}

	public void ProduceResource(BlueprintColonyProject projectBp, ResourceData resourceData)
	{
		ColonyProject colonyProject = Projects.FirstOrDefault((ColonyProject p) => p.Blueprint == projectBp);
		if (colonyProject == null)
		{
			PFLog.Default.Warning("Cannot find project " + projectBp.Name + " in colony " + Blueprint.name);
			return;
		}
		colonyProject.ProducedResourcesWithoutModifiers.Add(resourceData.Resource.Get(), resourceData.Count);
		int producedResourceCountWithEfficiencyModifier = ColoniesStateHelper.GetProducedResourceCountWithEfficiencyModifier(resourceData.Count, Efficiency.Value);
		AddResourceFromColony(resourceData.Resource.Get(), producedResourceCountWithEfficiencyModifier);
	}

	public Dictionary<BlueprintResource, int> RequiredResourcesForColony()
	{
		Dictionary<BlueprintResource, int> dictionary = new Dictionary<BlueprintResource, int>();
		foreach (ColonyProject item in Projects.Where((ColonyProject proj) => proj.IsFinished))
		{
			foreach (KeyValuePair<BlueprintResource, int> item2 in item.UsedResourcesFromPool)
			{
				item2.Deconstruct(out var key, out var value);
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
		return dictionary;
	}

	public Dictionary<BlueprintResource, int> ProducedResourcesByColony()
	{
		Dictionary<BlueprintResource, int> dictionary = new Dictionary<BlueprintResource, int>();
		BlueprintResource key;
		int value;
		foreach (ColonyProject item in Projects.Where((ColonyProject proj) => proj.IsFinished))
		{
			foreach (KeyValuePair<BlueprintResource, int> producedResourcesWithoutModifier in item.ProducedResourcesWithoutModifiers)
			{
				producedResourcesWithoutModifier.Deconstruct(out key, out value);
				BlueprintResource blueprintResource = key;
				int producedResourceCountWithEfficiencyModifier = ColoniesStateHelper.GetProducedResourceCountWithEfficiencyModifier(value, Efficiency.Value);
				if (dictionary.ContainsKey(blueprintResource))
				{
					Dictionary<BlueprintResource, int> dictionary2 = dictionary;
					key = blueprintResource;
					dictionary2[key] += producedResourceCountWithEfficiencyModifier;
				}
				else
				{
					dictionary.Add(blueprintResource, producedResourceCountWithEfficiencyModifier);
				}
			}
		}
		foreach (KeyValuePair<BlueprintResource, int> initialResource in InitialResources)
		{
			initialResource.Deconstruct(out key, out value);
			BlueprintResource blueprintResource2 = key;
			int num = value;
			if (dictionary.ContainsKey(blueprintResource2))
			{
				Dictionary<BlueprintResource, int> dictionary2 = dictionary;
				key = blueprintResource2;
				dictionary2[key] += num;
			}
			else
			{
				dictionary.Add(blueprintResource2, num);
			}
		}
		return dictionary;
	}

	private void AddResourceFromColony(BlueprintResource resource, int resourceCount)
	{
		Dictionary<BlueprintResource, int> availableProducedResources = AvailableProducedResources;
		if (availableProducedResources.ContainsKey(resource))
		{
			availableProducedResources[resource] += resourceCount;
		}
		else
		{
			availableProducedResources.Add(resource, resourceCount);
		}
		Game.Instance.ColonizationController.RemoveResourceShortage(this, resource, resourceCount);
		EventBus.RaiseEvent(delegate(IColonizationResourcesHandler h)
		{
			h.HandleColonyResourcesUpdated(resource, resourceCount);
		});
	}

	private void RemoveResourceFromColony(BlueprintResource resource, int resourceCount)
	{
		Dictionary<BlueprintResource, int> availableProducedResources = AvailableProducedResources;
		if (availableProducedResources.TryGetValue(resource, out var value))
		{
			if (value - resourceCount == 0)
			{
				availableProducedResources.Remove(resource);
			}
			else if (value - resourceCount > 0)
			{
				availableProducedResources[resource] -= resourceCount;
			}
			else
			{
				availableProducedResources.Remove(resource);
				Game.Instance.ColonizationController.UseResourceFromPool(resource, resourceCount - value);
			}
			EventBus.RaiseEvent(delegate(IColonizationResourcesHandler h)
			{
				h.HandleColonyResourcesUpdated(resource, resourceCount);
			});
		}
		else
		{
			Game.Instance.ColonizationController.UseResourceFromPool(resource, resourceCount);
		}
	}

	public int UseResourceFromColony(BlueprintResource resource, int resourceCount)
	{
		if (!AvailableProducedResources.TryGetValue(resource, out var value))
		{
			return resourceCount;
		}
		int num = Math.Min(resourceCount, value);
		AvailableProducedResources[resource] -= num;
		if (!UsedProducedResources.ContainsKey(resource))
		{
			UsedProducedResources.Add(resource, num);
		}
		else
		{
			UsedProducedResources[resource] += num;
		}
		return resourceCount - num;
	}

	public void ChangeContentment(int modifierValue, ColonyStatModifierType modifierType = ColonyStatModifierType.Other, BlueprintScriptableObject modifier = null)
	{
		if (modifierValue != 0)
		{
			Contentment.Modifiers.Add(new ColonyStatModifier
			{
				Value = modifierValue,
				ModifierType = modifierType,
				Modifier = modifier
			});
			EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
			{
				h.HandleContentmentChanged(this, modifierValue);
			});
		}
	}

	public void ChangeSecurity(int modifierValue, ColonyStatModifierType modifierType = ColonyStatModifierType.Other, BlueprintScriptableObject modifier = null)
	{
		if (modifierValue != 0)
		{
			Security.Modifiers.Add(new ColonyStatModifier
			{
				Value = modifierValue,
				ModifierType = modifierType,
				Modifier = modifier
			});
			EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
			{
				h.HandleSecurityChanged(this, modifierValue);
			});
		}
	}

	public void ChangeEfficiency(int modifierValue, ColonyStatModifierType modifierType = ColonyStatModifierType.Other, BlueprintScriptableObject modifier = null)
	{
		if (modifierValue == 0)
		{
			return;
		}
		int value = Efficiency.Value;
		Efficiency.Modifiers.Add(new ColonyStatModifier
		{
			Value = modifierValue,
			ModifierType = modifierType,
			Modifier = modifier
		});
		int value2 = Efficiency.Value;
		if (value != value2)
		{
			foreach (ColonyProject project in Projects)
			{
				if (!project.IsFinished)
				{
					continue;
				}
				foreach (KeyValuePair<BlueprintResource, int> producedResourcesWithoutModifier in project.ProducedResourcesWithoutModifiers)
				{
					producedResourcesWithoutModifier.Deconstruct(out var key, out var value3);
					BlueprintResource resource = key;
					int count = value3;
					int producedResourceCountWithEfficiencyModifier = ColoniesStateHelper.GetProducedResourceCountWithEfficiencyModifier(count, value);
					int producedResourceCountWithEfficiencyModifier2 = ColoniesStateHelper.GetProducedResourceCountWithEfficiencyModifier(count, value2);
					if (value < value2)
					{
						AddResourceFromColony(resource, producedResourceCountWithEfficiencyModifier2 - producedResourceCountWithEfficiencyModifier);
					}
					else
					{
						RemoveResourceFromColony(resource, producedResourceCountWithEfficiencyModifier - producedResourceCountWithEfficiencyModifier2);
					}
				}
			}
		}
		EventBus.RaiseEvent(delegate(IColonizationEachStatHandler h)
		{
			h.HandleEfficiencyChanged(this, modifierValue);
		});
	}

	public int SegmentsToBuildProject(BlueprintColonyProject projectBp)
	{
		return Mathf.FloorToInt((1f - (float)Contentment.Value / 10f) * (float)projectBp.SegmentsToBuild);
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<ColonyProject> projects = Projects;
		if (projects != null)
		{
			for (int i = 0; i < projects.Count; i++)
			{
				Hash128 val = ClassHasher<ColonyProject>.GetHash128(projects[i]);
				result.Append(ref val);
			}
		}
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Planet);
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val3);
		result.Append(ref ColonyFoundationTime);
		Hash128 val4 = ClassHasher<ColonyStat>.GetHash128(Contentment);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<ColonyStat>.GetHash128(Efficiency);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<ColonyStat>.GetHash128(Security);
		result.Append(ref val6);
		Dictionary<BlueprintResource, int> availableProducedResources = AvailableProducedResources;
		if (availableProducedResources != null)
		{
			int val7 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item in availableProducedResources)
			{
				Hash128 hash = default(Hash128);
				Hash128 val8 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val8);
				int obj = item.Value;
				Hash128 val9 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val9);
				val7 ^= hash.GetHashCode();
			}
			result.Append(ref val7);
		}
		Dictionary<BlueprintResource, int> initialResources = InitialResources;
		if (initialResources != null)
		{
			int val10 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item2 in initialResources)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val11);
				int obj2 = item2.Value;
				Hash128 val12 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val12);
				val10 ^= hash2.GetHashCode();
			}
			result.Append(ref val10);
		}
		Dictionary<BlueprintResource, int> usedProducedResources = UsedProducedResources;
		if (usedProducedResources != null)
		{
			int val13 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item3 in usedProducedResources)
			{
				Hash128 hash3 = default(Hash128);
				Hash128 val14 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item3.Key);
				hash3.Append(ref val14);
				int obj3 = item3.Value;
				Hash128 val15 = UnmanagedHasher<int>.GetHash128(ref obj3);
				hash3.Append(ref val15);
				val13 ^= hash3.GetHashCode();
			}
			result.Append(ref val13);
		}
		Dictionary<BlueprintColonyTrait, TimeSpan> colonyTraits = ColonyTraits;
		if (colonyTraits != null)
		{
			int val16 = 0;
			foreach (KeyValuePair<BlueprintColonyTrait, TimeSpan> item4 in colonyTraits)
			{
				Hash128 hash4 = default(Hash128);
				Hash128 val17 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item4.Key);
				hash4.Append(ref val17);
				TimeSpan obj4 = item4.Value;
				Hash128 val18 = UnmanagedHasher<TimeSpan>.GetHash128(ref obj4);
				hash4.Append(ref val18);
				val16 ^= hash4.GetHashCode();
			}
			result.Append(ref val16);
		}
		Hash128 val19 = ClassHasher<BlueprintColonyEventsRoot.ColonyEventToTimer>.GetHash128(m_CurrentEvent);
		result.Append(ref val19);
		List<BlueprintColonyEventsRoot.ColonyEventToTimer> allEventsForColony = AllEventsForColony;
		if (allEventsForColony != null)
		{
			for (int j = 0; j < allEventsForColony.Count; j++)
			{
				Hash128 val20 = ClassHasher<BlueprintColonyEventsRoot.ColonyEventToTimer>.GetHash128(allEventsForColony[j]);
				result.Append(ref val20);
			}
		}
		List<BlueprintColonyEventsRoot.ColonyEventToTimer> notStartedEvents = m_NotStartedEvents;
		if (notStartedEvents != null)
		{
			for (int k = 0; k < notStartedEvents.Count; k++)
			{
				Hash128 val21 = ClassHasher<BlueprintColonyEventsRoot.ColonyEventToTimer>.GetHash128(notStartedEvents[k]);
				result.Append(ref val21);
			}
		}
		List<BlueprintColonyEvent> startedEvents = StartedEvents;
		if (startedEvents != null)
		{
			for (int l = 0; l < startedEvents.Count; l++)
			{
				Hash128 val22 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(startedEvents[l]);
				result.Append(ref val22);
			}
		}
		result.Append(ref m_LastEventTime);
		List<BlueprintDialog> startedChronicles = StartedChronicles;
		if (startedChronicles != null)
		{
			for (int m = 0; m < startedChronicles.Count; m++)
			{
				Hash128 val23 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(startedChronicles[m]);
				result.Append(ref val23);
			}
		}
		List<ColonyChronicle> chronicles = Chronicles;
		if (chronicles != null)
		{
			for (int n = 0; n < chronicles.Count; n++)
			{
				Hash128 val24 = ClassHasher<ColonyChronicle>.GetHash128(chronicles[n]);
				result.Append(ref val24);
			}
		}
		Hash128 val25 = ClassHasher<ColonyLootHolder>.GetHash128(LootToReceive);
		result.Append(ref val25);
		List<Consumable> consumables = Consumables;
		if (consumables != null)
		{
			for (int num = 0; num < consumables.Count; num++)
			{
				Hash128 val26 = ClassHasher<Consumable>.GetHash128(consumables[num]);
				result.Append(ref val26);
			}
		}
		List<ColonyProject> finishedProjectsSinceLastVisit = FinishedProjectsSinceLastVisit;
		if (finishedProjectsSinceLastVisit != null)
		{
			for (int num2 = 0; num2 < finishedProjectsSinceLastVisit.Count; num2++)
			{
				Hash128 val27 = ClassHasher<ColonyProject>.GetHash128(finishedProjectsSinceLastVisit[num2]);
				result.Append(ref val27);
			}
		}
		return result;
	}
}
