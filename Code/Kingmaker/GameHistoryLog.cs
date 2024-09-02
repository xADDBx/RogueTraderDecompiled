using System;
using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cheats;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SectorMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View;
using Kingmaker.View.MapObjects.SriptZones;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class GameHistoryLog : IQuestHandler, ISubscriber, IQuestObjectiveHandler, IUnlockHandler, IUnlockValueHandler, IItemsCollectionHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IPartyHandler, ISubscriber<IBaseUnitEntity>, ICompanionChangeHandler, ILevelUpCompleteUIHandler, ICutsceneHandler, ISubscriber<CutscenePlayerData>, IScriptZoneHandler, IPartyCombatHandler, IRollSkillCheckHandler, IUnitFactionHandler, ISelectAnswerHandler, IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, IGlobalRulebookSubscriber, ISectorMapScanHandler, ISubscriber<ISectorMapObjectEntity>, ISectorMapWarpTravelHandler, ISectorMapPassageChangeHandler, ISubscriber<ISectorMapPassageEntity>, IColonizationHandler, IColonizationProjectsHandler, IColonizationTraitHandler, IExplorationHandler, IAnomalyHandler, ISubscriber<AnomalyEntityData>, ICombatRandomEncounterHandler, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>
{
	[CanBeNull]
	private static GameHistoryLog s_Instance;

	[NotNull]
	public static GameHistoryLog Instance
	{
		get
		{
			Initialize();
			return s_Instance;
		}
	}

	public static void Initialize()
	{
		if (s_Instance == null)
		{
			s_Instance = new GameHistoryLog();
			EventBus.Subscribe(s_Instance);
			GameCoreHistoryLog instance = GameCoreHistoryLog.Instance;
			instance.EtudeEventAction = (Action<UnityEngine.Object, string>)Delegate.Combine(instance.EtudeEventAction, new Action<UnityEngine.Object, string>(s_Instance.EtudeEvent));
		}
	}

	public void DialogEvent(UnityEngine.Object context, string message)
	{
		if (!(context.name == "DefaultContinue"))
		{
			AddMessage(PFLog.History.Dialog, context, message);
		}
	}

	public void DialogEvent(ICanBeLogContext context, string message)
	{
		if (!((context as SimpleBlueprint)?.name == "DefaultContinue"))
		{
			AddMessage(PFLog.History.Dialog, context, message);
		}
	}

	public void EtudeEvent(UnityEngine.Object context, string message)
	{
		AddMessage(PFLog.History.Etudes, context, message);
	}

	public void SystemEvent(string message)
	{
		AddMessage(PFLog.History.System, (ICanBeLogContext)null, message);
	}

	public void AreaLoading(BlueprintArea oldArea, BlueprintArea newArea, SceneReference[] scenes)
	{
		if (oldArea != newArea)
		{
			AddMessage(PFLog.History.Area, oldArea, "unloading area");
			AddMessage(PFLog.History.Area, newArea, "loading area");
		}
		else
		{
			AddMessage(PFLog.History.Area, newArea, "reloading area");
		}
		foreach (SceneReference sceneReference in scenes)
		{
			AddMessage(PFLog.History.Area, (ICanBeLogContext)null, "scene: " + sceneReference.SceneName);
		}
	}

	public void HandleQuestStarted(Quest quest)
	{
		AddMessage(PFLog.History.Quests, quest.Blueprint, "quest started");
	}

	public void HandleQuestCompleted(Quest quest)
	{
		AddMessage(PFLog.History.Quests, quest.Blueprint, "quest completed");
	}

	public void HandleQuestFailed(Quest quest)
	{
		AddMessage(PFLog.History.Quests, quest.Blueprint, "quest failed");
	}

	public void HandleQuestUpdated(Quest objective)
	{
	}

	public void HandleQuestObjectiveStarted(QuestObjective objective, bool silentStart = false)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective started");
	}

	public void HandleQuestObjectiveBecameVisible(QuestObjective objective, bool silentStart = false)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective became visible");
	}

	public void HandleQuestObjectiveCompleted(QuestObjective objective)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective completed");
	}

	public void HandleQuestObjectiveFailed(QuestObjective objective)
	{
		AddMessage(PFLog.History.Quests, objective.Blueprint, "objective failed");
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		AddMessage(PFLog.History.Unlocks, flag, "unlocked");
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		AddMessage(PFLog.History.Unlocks, flag, "locked");
	}

	public void HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		AddMessage(PFLog.History.Unlocks, flag, $"set to {value}");
	}

	public void OnNewDay()
	{
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection == Game.Instance.Player.Inventory && !(item?.Owner?.GetBodyOptional()?.AdditionalLimbs?.Contains(item.HoldingSlot)).GetValueOrDefault())
		{
			string message = ((count == 1) ? "item found" : $"{count} items found");
			AddMessage(PFLog.History.Items, item?.Blueprint, message);
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (collection == Game.Instance.Player.Inventory && !(item?.Owner?.GetBodyOptional()?.AdditionalLimbs?.Contains(item.HoldingSlot)).GetValueOrDefault())
		{
			string message = ((count == 1) ? "item lost" : $"{count} items lost");
			AddMessage(PFLog.History.Items, item?.Blueprint, message);
		}
	}

	public void HandleUnitSpawned()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !abstractUnitEntity.IsPlayerFaction)
		{
			AddMessage(PFLog.History.Area, abstractUnitEntity.Blueprint, "unit spawned");
		}
	}

	public void HandleUnitDestroyed()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null && !abstractUnitEntity.IsPlayerFaction)
		{
			AddMessage(PFLog.History.Area, abstractUnitEntity.Blueprint, "unit destroyed");
		}
	}

	public void HandleUnitDeath()
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity != null)
		{
			if (abstractUnitEntity.IsPlayerFaction)
			{
				AddMessage(PFLog.History.Party, abstractUnitEntity.Blueprint, "companion dies");
			}
			else
			{
				AddMessage(PFLog.History.Area, abstractUnitEntity.Blueprint, "unit dies");
			}
		}
	}

	public void HandleAddCompanion()
	{
		AddMessage(PFLog.History.Party, EventInvokerExtensions.BaseUnitEntity.Blueprint, "add companion");
	}

	public void HandleCompanionActivated()
	{
		AddMessage(PFLog.History.Party, EventInvokerExtensions.BaseUnitEntity.Blueprint, "activate companion");
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
		AddMessage(PFLog.History.Party, EventInvokerExtensions.BaseUnitEntity.Blueprint, "remove companion");
	}

	public void HandleCapitalModeChanged()
	{
	}

	public void HandleRecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Party, baseUnitEntity.Blueprint, "recruit companion");
	}

	public void HandleUnrecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Party, baseUnitEntity.Blueprint, "unrecruit companion");
	}

	public void HandleLevelUpComplete(bool isChargen)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Party, baseUnitEntity.Blueprint, $"level up: {baseUnitEntity.Progression.CharacterLevel}");
	}

	public void HandleCutsceneStarted(bool queued)
	{
		string text = "cutscene started";
		if (queued)
		{
			text += " (queued)";
		}
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, text);
	}

	public void HandleCutsceneRestarted()
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene restarted");
	}

	public void HandleCutscenePaused(CutscenePauseReason reason)
	{
		string name = Enum.GetName(typeof(CutscenePauseReason), reason);
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene paused because of " + name);
	}

	public void HandleCutsceneResumed()
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene resumed");
	}

	public void HandleCutsceneStopped()
	{
		CutscenePlayerData cutscenePlayerData = EventInvokerExtensions.Entity as CutscenePlayerData;
		AddMessage(PFLog.History.Area, cutscenePlayerData.Cutscene, "cutscene stopped");
	}

	public void OnUnitEnteredScriptZone(ScriptZone zone)
	{
		AddMessage(PFLog.History.Area, zone, $"{EventInvokerExtensions.BaseUnitEntity.Blueprint} enters script zone");
	}

	public void OnUnitExitedScriptZone(ScriptZone zone)
	{
		AddMessage(PFLog.History.Area, zone, $"{EventInvokerExtensions.BaseUnitEntity.Blueprint} leaves script zone");
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			List<BlueprintUnit> list = (from u in Game.Instance.State.AllBaseUnits
				where !u.Faction.IsPlayer
				where u.IsInCombat
				select u.Blueprint).ToList();
			int totalChallengeRating = Utilities.GetTotalChallengeRating(list);
			AddMessage(PFLog.History.Combat, (ICanBeLogContext)null, $"party combat started | enemies count = {list.Count} | enemies cr = {totalChallengeRating}");
		}
		else
		{
			AddMessage(PFLog.History.Combat, (ICanBeLogContext)null, "party combat finished");
		}
	}

	public void HandlePartySkillCheckRolled(RulePerformPartySkillCheck check)
	{
		string text = (check.Success ? "passed" : "failed");
		AddMessage(PFLog.History.Skill, check.Roller?.Blueprint, $"{check.StatType} check {text} (party) | dc = {check.Difficulty} | result = {check.RollResult} | d100 = {check.D100} | stat value = {check.StatValue}");
	}

	public void HandleUnitSkillCheckRolled(RulePerformSkillCheck check)
	{
	}

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSkillCheck check)
	{
		if (!(check is RuleCachedPerceptionCheck))
		{
			string text = (check.ResultIsSuccess ? "passed" : "failed");
			AddMessage(PFLog.History.Skill, check.ConcreteInitiator?.Blueprint, $"{check.StatType} check {text} | dc = {check.Difficulty} | result = {check.RollResult} | d100 = {check.ResultChanceRule} | stat value = {check.StatValue}");
		}
	}

	public void HandleFactionChanged()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		AddMessage(PFLog.History.Area, baseUnitEntity?.Blueprint, $"{baseUnitEntity} faction changed to {baseUnitEntity?.Faction}");
	}

	public void HandleSelectAnswer(BlueprintAnswer answer)
	{
		DialogEvent(answer, "Selected answer");
	}

	private static void AddMessage(LogChannel channel, [CanBeNull] UnityEngine.Object context, string message)
	{
		CalendarRoot calendar = BlueprintRoot.Instance.Calendar;
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		DateTime dateTime = calendar.GetStartDate() + gameTime;
		int num = dateTime.Year + calendar.YearsShift;
		string text = $"{dateTime.Hour:D2}:{dateTime.Minute:D2}:{dateTime.Second:D2} {dateTime.Day:D2}.{dateTime.Month:D2}.{num}";
		string text2 = "";
		if (context != null)
		{
			text2 = ((!(context is EntityViewBase entityViewBase)) ? (context.name + ": ") : (entityViewBase.name + " [" + entityViewBase.UniqueId + "]"));
		}
		channel.Log(context, "{0} - {1}{2}", text, text2, message);
	}

	private static void AddMessage(LogChannel channel, [CanBeNull] ICanBeLogContext context, string message)
	{
		CalendarRoot calendar = BlueprintRoot.Instance.Calendar;
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		DateTime dateTime = calendar.GetStartDate() + gameTime;
		int num = dateTime.Year + calendar.YearsShift;
		string text = $"{dateTime.Hour:D2}:{dateTime.Minute:D2}:{dateTime.Second:D2} {dateTime.Day:D2}.{dateTime.Month:D2}.{num}";
		string text2 = "";
		if (context != null)
		{
			text2 = ((!(context is BlueprintScriptableObject blueprintScriptableObject)) ? $"{context}: " : (blueprintScriptableObject.name + " (" + blueprintScriptableObject.AssetGuid + "): "));
		}
		channel.Log(context, "{0} - {1}{2}", text, text2, message);
	}

	public void HandleScanStarted(float range, float duration)
	{
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
	}

	public void HandleScanStopped()
	{
		SectorMapObjectEntity sectorMapObjectEntity = EventInvokerExtensions.Entity as SectorMapObjectEntity;
		AddMessage(PFLog.History.SectorMap, sectorMapObjectEntity?.Blueprint, "scanned from");
	}

	public void HandleWarpTravelBeforeStart()
	{
	}

	public void HandleWarpTravelStarted(SectorMapPassageEntity passage)
	{
		SectorMapObjectEntity sectorMapObjectEntity = EventInvokerExtensions.Entity as SectorMapObjectEntity;
		BlueprintSectorMapPoint blueprintSectorMapPoint = ((passage.View.StarSystem1.Blueprint == sectorMapObjectEntity?.Blueprint) ? passage.View.StarSystem2.Blueprint : passage.View.StarSystem1.Blueprint);
		AddMessage(PFLog.History.SectorMap, sectorMapObjectEntity?.Blueprint, $"started travel to {blueprintSectorMapPoint?.name} ({blueprintSectorMapPoint?.AssetGuid}), passage difficulty {passage.CurrentDifficulty}, duration {passage.DurationInDays} segments");
	}

	public void HandleWarpTravelStopped()
	{
		AddMessage(PFLog.History.SectorMap, (ICanBeLogContext)null, "travel stopped");
	}

	public void HandleWarpTravelPaused()
	{
	}

	public void HandleWarpTravelResumed()
	{
	}

	public void HandleNewPassageCreated()
	{
		SectorMapPassageEntity sectorMapPassageEntity = EventInvokerExtensions.Entity as SectorMapPassageEntity;
		AddMessage(PFLog.History.SectorMap, (ICanBeLogContext)null, "created passage from " + sectorMapPassageEntity?.View.StarSystem1.Blueprint.name + " to " + sectorMapPassageEntity?.View.StarSystem2.Blueprint.name);
	}

	public void HandlePassageLowerDifficulty()
	{
		SectorMapPassageEntity sectorMapPassageEntity = EventInvokerExtensions.Entity as SectorMapPassageEntity;
		AddMessage(PFLog.History.SectorMap, (ICanBeLogContext)null, $"lowered difficulty for passage from {sectorMapPassageEntity?.View.StarSystem1.Blueprint.name} to {sectorMapPassageEntity?.View.StarSystem2.Blueprint.name}, current difficulty: {sectorMapPassageEntity?.CurrentDifficulty}");
	}

	public void HandleColonyCreated(Colony colony, PlanetEntity planetEntity)
	{
		AddMessage(PFLog.History.Colonization, colony.Blueprint, "colony created");
	}

	public void HandleColonyProjectStarted(Colony colony, ColonyProject project)
	{
		AddMessage(PFLog.History.Colonization, colony.Blueprint, "started project $" + project.Blueprint.name + " (" + project.Blueprint.AssetGuid + ")");
	}

	public void HandleColonyProjectFinished(Colony colony, ColonyProject project)
	{
		AddMessage(PFLog.History.Colonization, colony.Blueprint, "finished project " + project.Blueprint.name + " (" + project.Blueprint.AssetGuid + ")");
	}

	public void HandleTraitStarted(Colony colony, BlueprintColonyTrait trait)
	{
		AddMessage(PFLog.History.Colonization, colony.Blueprint, "added trait " + trait.name + " (" + trait.AssetGuid + ")");
	}

	public void HandleTraitEnded(Colony colony, BlueprintColonyTrait trait)
	{
		AddMessage(PFLog.History.Colonization, colony.Blueprint, "trait finished " + trait.name + " (" + trait.AssetGuid + ")");
	}

	public void HandlePointOfInterestInteracted(BasePointOfInterest pointOfInterest)
	{
		AddMessage(PFLog.History.StarSystemMap, (ICanBeLogContext)null, "point of interest " + pointOfInterest.Blueprint.name + " (" + pointOfInterest.Blueprint.AssetGuid + ") interacted");
	}

	public void HandleAnomalyInteracted()
	{
		AnomalyEntityData anomalyEntityData = EventInvokerExtensions.Entity as AnomalyEntityData;
		AddMessage(PFLog.History.StarSystemMap, anomalyEntityData?.Blueprint, "anomaly interacted");
	}

	public void HandleCombatRandomEncounterStart()
	{
		AddMessage(PFLog.History.SectorMap, (ICanBeLogContext)null, "combat random encounter started");
	}

	public void HandleCombatRandomEncounterFinish()
	{
		AddMessage(PFLog.History.SectorMap, (ICanBeLogContext)null, "combat random encounter finished");
	}

	public void HandleStartScanningStarSystemObject()
	{
	}

	public void HandleScanStarSystemObject()
	{
		StarSystemObjectEntity starSystemObjectEntity = EventInvokerExtensions.Entity as StarSystemObjectEntity;
		AddMessage(PFLog.History.StarSystemMap, starSystemObjectEntity?.Blueprint, "sso scanned");
	}
}
