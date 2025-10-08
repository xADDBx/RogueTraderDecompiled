using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Cargo;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.SavesStorage;
using Kingmaker.GameCommands.Colonization;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.CharacterSystem;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.GameCommands;

public static class GameCommandQueueExtensions
{
	public static void SkipBark([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new SkipBarkGameCommand());
	}

	public static void SkipCutscene([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new SkipCutsceneGameCommand());
	}

	public static void ScheduleSwitchCutsceneLock([NotNull] this GameCommandQueue gameCommandQueue, bool @lock)
	{
		gameCommandQueue.AddCommand(new SwitchCutsceneLockCommand(@lock));
	}

	public static void ScheduleDialogStart([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] DialogData dialog)
	{
		gameCommandQueue.AddCommand(new StartScheduledDialogCommand(dialog));
	}

	public static void SchedulePostSaveCallback([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] SaveInfo saveInfo, [NotNull] SaveCreateDTO dto)
	{
		PFLog.System.Log("Post-save callback requested");
		gameCommandQueue.AddCommand(new PostSaveCallbackCommand(saveInfo, dto));
	}

	public static void EndTurnManually([NotNull] this GameCommandQueue gameCommandQueue, MechanicEntity entity)
	{
		if (!gameCommandQueue.ContainsCommand((EndTurnGameCommand cmd) => cmd.MechanicEntity == entity))
		{
			gameCommandQueue.AddCommand(new EndTurnGameCommand(entity));
		}
	}

	public static void SetPauseManualState([NotNull] this GameCommandQueue gameCommandQueue, bool toPause)
	{
		gameCommandQueue.AddCommand(new SetPauseGameCommand(toPause));
	}

	public static void RequestPauseUi([NotNull] this GameCommandQueue gameCommandQueue, bool toPause)
	{
		if (!gameCommandQueue.ContainsCommand((RequestPauseGameCommand cmd) => cmd.ToPause == toPause))
		{
			gameCommandQueue.AddCommand(new RequestPauseGameCommand(toPause));
		}
	}

	public static void DialogAnswer([NotNull] this GameCommandQueue gameCommandQueue, int tick, [NotNull] string answer)
	{
		gameCommandQueue.AddCommand(new DialogAnswerGameCommand(tick, answer));
	}

	public static void AreaTransition([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] AreaTransitionPart areaTransitionPart, bool isPlayerCommand, BaseUnitEntity executorEntity)
	{
		gameCommandQueue.AddCommand(new AreaTransitionPartGameCommand(areaTransitionPart, isPlayerCommand, executorEntity));
	}

	public static void AreaTransition([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintMultiEntranceEntry multiEntrance)
	{
		gameCommandQueue.AddCommand(new AreaTransitionGameCommand(multiEntrance));
	}

	public static void ClearAreaTransitionGroupDuplicates([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new ClearAreaTransitionGroupGameCommand());
	}

	public static void DropItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, bool split, int splitCount)
	{
		gameCommandQueue.AddCommand(new DropItemGameCommand(item, split, splitCount));
	}

	public static void EquipItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, [NotNull] MechanicEntity unit, ItemSlotRef to)
	{
		gameCommandQueue.AddCommand(new EquipItemGameCommand(item, unit, to));
	}

	public static void UnequipItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] MechanicEntity owner, [NotNull] ItemSlotRef from, ItemSlotRef to)
	{
		gameCommandQueue.AddCommand(new UnequipItemGameCommand(owner, from, to));
	}

	public static void AddRemoveItemAsFavorite([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item)
	{
		gameCommandQueue.AddCommand(new AddRemoveItemAsFavoriteCommand(item));
	}

	public static void SwapSlots([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] MechanicEntity entity, [NotNull] ItemSlotRef from, [NotNull] ItemSlotRef to, bool isLoot)
	{
		gameCommandQueue.AddCommand(new SwapSlotsGameCommand(entity, from, to, isLoot));
	}

	public static void SplitSlot([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemSlotRef from, ItemSlotRef to, bool isLoot, int count)
	{
		gameCommandQueue.AddCommand(new SplitSlotGameCommand(from, to, isLoot, count));
	}

	public static void MergeSlot([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemSlotRef from, [NotNull] ItemSlotRef to)
	{
		gameCommandQueue.AddCommand(new MergeSlotGameCommand(from, to));
	}

	public static void SwitchHandEquipment([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BaseUnitEntity unit, int handEquipmentSetIndex)
	{
		gameCommandQueue.AddCommand(new SwitchHandEquipmentGameCommand(unit, handEquipmentSetIndex));
	}

	public static void RemoveFromBuyVendor([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, int count)
	{
		gameCommandQueue.AddCommand(new RemoveFromBuyVendorGameCommand(item, count));
	}

	public static void AddForBuyVendor([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, int count, bool makeDeal = false)
	{
		gameCommandQueue.AddCommand(new AddForBuyVendorGameCommand(item, count, makeDeal));
	}

	public static void TransferItem([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] ItemEntity item, [NotNull] ItemsCollectionRef to, int count)
	{
		gameCommandQueue.AddCommand(new TransferItemGameCommand(to, item, count));
	}

	public static void CollectLoot([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<EntityRef<ItemEntity>> items)
	{
		gameCommandQueue.AddCommand(new CollectLootGameCommand(items));
	}

	public static void CreateCargoInternal([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintCargo cargoBlueprint)
	{
		gameCommandQueue.AddCommand(new CreateCargoInternalGameCommand(cargoBlueprint));
	}

	public static void TransferItemsToCargo([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<EntityRef<ItemEntity>> items)
	{
		gameCommandQueue.AddCommand(new TransferItemsToCargoGameCommand(items));
	}

	public static void TransferItemsToInventory([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<EntityRef<ItemEntity>> items)
	{
		gameCommandQueue.AddCommand(new TransferItemToInventoryGameCommand(items));
	}

	public static void AddCargoToSell([NotNull] this GameCommandQueue gameCommandQueue, CargoEntity cargo)
	{
		gameCommandQueue.AddCommand(new AddCargoToSellGameCommand(cargo));
	}

	public static void RemoveCargoFromSell([NotNull] this GameCommandQueue gameCommandQueue, CargoEntity cargo)
	{
		gameCommandQueue.AddCommand(new RemoveCargoFromSellGameCommand(cargo));
	}

	public static void DealSellCargoes([NotNull] this GameCommandQueue gameCommandQueue, MechanicEntity vendor)
	{
		gameCommandQueue.AddCommand(new DealSellCargoesGameCommand(vendor));
	}

	public static void StartTrading([NotNull] this GameCommandQueue gameCommandQueue, MechanicEntity vendor, bool isSynchronized)
	{
		gameCommandQueue.AddCommand(new StartTradingGameCommand(vendor, isSynchronized));
	}

	public static void EndTrading([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new EndTradingGameCommand());
	}

	public static void SaveGame([NotNull] this GameCommandQueue gameCommandQueue, [CanBeNull] SaveInfo saveInfo, [CanBeNull] string saveName = null, Action callback = null)
	{
		gameCommandQueue.AddCommand(new SaveGameCommand(saveInfo, saveName, callback));
	}

	public static void SetInventorySorter([NotNull] this GameCommandQueue gameCommandQueue, ItemsSorterType sorterType)
	{
		gameCommandQueue.AddCommand(new SetInventorySorterGameCommand(sorterType));
	}

	public static void ScanStarSystemObject([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] StarSystemObjectEntity starSystemObject, bool finishScan = false)
	{
		gameCommandQueue.AddCommand(new ScanStarSystemObjectGameCommand(starSystemObject, finishScan));
	}

	public static void CloseExplorationScreen([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new CloseExplorationScreenCommand());
	}

	public static void CloseScreen([NotNull] this GameCommandQueue gameCommandQueue, IScreenUIHandler.ScreenType screenType, bool isSynchronized = true)
	{
		gameCommandQueue.AddCommand(new CloseScreenCommand(screenType, isSynchronized));
	}

	public static void SetCurrentQuest([NotNull] this GameCommandQueue gameCommandQueue, Quest quest)
	{
		gameCommandQueue.AddCommand(new SetCurrentQuestGameCommand(quest));
	}

	public static void CommitLvlUp([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] LevelUpManager levelUpManager)
	{
		if (levelUpManager.Path != null)
		{
			gameCommandQueue.AddCommand(new CommitLevelUpGameCommand(levelUpManager));
		}
	}

	public static void PointOfInterestInteract([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] StarSystemObjectEntity starSystemObject, BlueprintPointOfInterest poi)
	{
		gameCommandQueue.AddCommand(new PointOfInterestInteractGameCommand(starSystemObject, poi));
	}

	public static void PointOfInterestSetInteracted([NotNull] this GameCommandQueue gameCommandQueue, BlueprintPointOfInterest poi)
	{
		gameCommandQueue.AddCommand(new PointOfInterestSetInteractedGameCommand(poi));
	}

	public static void ScanOnSectorMap([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new ScanOnSectorMapGameCommand());
	}

	public static void CreateNewWarpRoute([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] SectorMapObjectEntity from, [NotNull] SectorMapObjectEntity to)
	{
		gameCommandQueue.AddCommand(new CreateNewWarpRouteGameCommand(from, to));
	}

	public static void LowerWarpRouteDifficulty([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] SectorMapObjectEntity to, [NotNull] SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		gameCommandQueue.AddCommand(new LowerWarpRouteDifficultyGameCommand(to, difficulty));
	}

	public static void CreateColony([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] PlanetEntity planet, List<BlueprintColonyEvent> events, bool isPlayerCommand)
	{
		gameCommandQueue.AddCommand(new CreateColonyGameCommand(planet, events, isPlayerCommand));
	}

	public static void StartColonyProject([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintColonyReference colony, [NotNull] BlueprintColonyProjectReference project)
	{
		gameCommandQueue.AddCommand(new StartColonyProjectGameCommand(colony, project));
	}

	public static void StartColonyEvent([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintColony colony, [NotNull] BlueprintColonyEvent colonyEvent)
	{
		gameCommandQueue.AddCommand(new StartColonyEventGameCommand(colony, colonyEvent));
	}

	public static void StartWarpTravel([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] SectorMapObjectEntity from, [NotNull] SectorMapObjectEntity to)
	{
		gameCommandQueue.AddCommand(new StartWarpTravelGameCommand(from, to));
	}

	public static void AcceptChangeGroup([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] List<UnitReference> partyCharacters, [NotNull] List<UnitReference> remoteCharacters, [NotNull] List<BlueprintUnitReference> requiredCharacters, bool isCapital, bool reInitPartyCharacters)
	{
		gameCommandQueue.AddCommand(new AcceptChangeGroupGameCommand(partyCharacters, remoteCharacters, requiredCharacters, isCapital, reInitPartyCharacters));
	}

	public static void LoadArea([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintAreaEnterPoint enterPoint, AutoSaveMode autoSaveMode)
	{
		gameCommandQueue.LoadArea(enterPoint.ToReference<BlueprintAreaEnterPointReference>(), autoSaveMode);
	}

	public static void LoadArea([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintAreaEnterPointReference enterPoint, AutoSaveMode autoSaveMode)
	{
		gameCommandQueue.AddCommand(new LoadAreaGameCommand(enterPoint, autoSaveMode));
	}

	public static void MoveShip([NotNull] this GameCommandQueue gameCommandQueue, StarSystemObjectView starSystemObjectView, MoveShipGameCommand.VisitType visitType)
	{
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(starSystemObjectView.Data);
		}) || !UINetUtility.IsControlMainCharacter())
		{
			return;
		}
		if (starSystemObjectView == null)
		{
			PFLog.Space.Error("starSystemObjectView is null!");
			return;
		}
		StarSystemObjectEntity data = starSystemObjectView.Data;
		if (data == null)
		{
			PFLog.Space.Error("AnomalyEntityData is null! View='" + starSystemObjectView.transform.GetHierarchyPath() + "'");
		}
		else
		{
			gameCommandQueue.AddCommand(new MoveShipGameCommand(data, visitType));
		}
	}

	public static void MoveShip([NotNull] this GameCommandQueue gameCommandQueue, MapObjectEntity mapObjectEntity, MoveShipGameCommand.VisitType visitType)
	{
		if (!PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingEntity(mapObjectEntity);
		}) && UINetUtility.IsControlMainCharacter())
		{
			if (mapObjectEntity == null)
			{
				PFLog.Space.Error("mapObjectEntity is null!");
			}
			else if (!(mapObjectEntity is StarSystemObjectEntity starSystemObjectEntity))
			{
				PFLog.Space.Error("mapObjectEntity is not StarSystemObjectEntity! Type='" + mapObjectEntity.GetType().Name + "'");
			}
			else
			{
				gameCommandQueue.AddCommand(new MoveShipGameCommand(starSystemObjectEntity, visitType));
			}
		}
	}

	public static void InteractWithStarSystemObjectGameCommand([NotNull] this GameCommandQueue gameCommandQueue, StarSystemObjectEntity starSystemObjectEntity, Vector3 position)
	{
		gameCommandQueue.AddCommand(new InteractWithStarSystemObjectGameCommand(starSystemObjectEntity, position));
	}

	public static void SetSettings([NotNull] this GameCommandQueue gameCommandQueue, List<BaseSettingNetData> settingCommand)
	{
		gameCommandQueue.AddCommand(new SettingGameCommand(settingCommand));
	}

	public static void DoSpeedUp([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new SpeedUpGameCommand());
	}

	public static void StopSpeedUp([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new StopSpeedUpGameCommand());
	}

	public static void UpgradeSystemComponent([NotNull] this GameCommandQueue gameCommandQueue, SystemComponent.SystemComponentType componentType)
	{
		gameCommandQueue.AddCommand(new UpgradeSystemComponentGameCommand(componentType));
	}

	public static void DowngradeSystemComponent([NotNull] this GameCommandQueue gameCommandQueue, SystemComponent.SystemComponentType componentType)
	{
		gameCommandQueue.AddCommand(new DowngradeSystemComponentGameCommand(componentType));
	}

	public static void PingPosition([NotNull] this GameCommandQueue gameCommandQueue, Vector3 position)
	{
		gameCommandQueue.AddCommand(new PingPositionGameCommand(position));
	}

	public static void PingEntity([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] Entity entity)
	{
		gameCommandQueue.AddCommand(new PingEntityGameCommand(entity));
	}

	public static void PingDialogAnswer([NotNull] this GameCommandQueue gameCommandQueue, string answer, bool isHover)
	{
		gameCommandQueue.AddCommand(new PingDialogAnswerGameCommand(answer, isHover));
	}

	public static void PingDialogAnswerVote([NotNull] this GameCommandQueue gameCommandQueue, string answer)
	{
		gameCommandQueue.AddCommand(new PingDialogAnswerVoteGameCommand(answer));
	}

	public static void PingActionBarAbility([NotNull] this GameCommandQueue gameCommandQueue, string keyName, Entity characterEntityRef, int slotIndex, WeaponSlotType weaponSlotType)
	{
		gameCommandQueue.AddCommand(new PingActionBarAbilityGameCommand(keyName, characterEntityRef, slotIndex, weaponSlotType));
	}

	public static void ChangePlayerRole([NotNull] this GameCommandQueue gameCommandQueue, string entityId, NetPlayer player, bool enable)
	{
		gameCommandQueue.AddCommand(new ChangePlayerRoleGameCommand(entityId, player, enable));
	}

	public static void StopStarSystemStarShip([NotNull] this GameCommandQueue gameCommandQueue)
	{
		bool flag = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
		if (!flag || UINetUtility.IsControlMainCharacter())
		{
			gameCommandQueue.AddCommand(new StopStarSystemStarShipGameCommand(flag));
		}
	}

	public static void UseResourceMiner([NotNull] this GameCommandQueue gameCommandQueue, BlueprintStarSystemObject sso, BlueprintResource resource)
	{
		gameCommandQueue.AddCommand(new UseResourceMinerGameCommand(sso, resource));
	}

	public static void RemoveResourceMiner([NotNull] this GameCommandQueue gameCommandQueue, BlueprintStarSystemObject sso, BlueprintResource resource)
	{
		gameCommandQueue.AddCommand(new RemoveResourceMinerGameCommand(sso.ToReference<BlueprintStarSystemObjectReference>(), resource.ToReference<BlueprintResourceReference>()));
	}

	public static void CompleteContract([NotNull] this GameCommandQueue gameCommandQueue, BlueprintQuest quest)
	{
		gameCommandQueue.AddCommand(new CompleteContractGameCommand(quest));
	}

	public static void SetQuestViewed([NotNull] this GameCommandQueue gameCommandQueue, Quest quest)
	{
		gameCommandQueue.AddCommand(new SetQuestViewedGameCommand(quest));
	}

	public static void SetQuestObjectiveViewed([NotNull] this GameCommandQueue gameCommandQueue, QuestObjective questObjective)
	{
		gameCommandQueue.AddCommand(new SetQuestObjectiveViewedGameCommand(questObjective));
	}

	public static void SetUnitOnPost([NotNull] this GameCommandQueue gameCommandQueue, BaseUnitEntity unit, PostType post, StarshipEntity starshipEntity)
	{
		gameCommandQueue.AddCommand(new SetUnitOnPostGameCommand(unit, post, starshipEntity));
	}

	public static void AttuneAbilityForPost(this GameCommandQueue gameCommandQueue, Post post, BlueprintAbility blueprintAbility)
	{
		gameCommandQueue.AddCommand(new AttuneAbilityForPostGameCommand(post, blueprintAbility));
	}

	public static void PartyFormationIndex([NotNull] this GameCommandQueue gameCommandQueue, int formationIndex)
	{
		gameCommandQueue.AddCommand(new PartyFormationIndexGameCommand(formationIndex));
	}

	public static void PartyFormationResetGameCommand([NotNull] this GameCommandQueue gameCommandQueue)
	{
		gameCommandQueue.AddCommand(new PartyFormationResetGameCommand(Game.Instance.Player.FormationManager.CurrentFormationIndex));
	}

	public static void PartyFormationOffset([NotNull] this GameCommandQueue gameCommandQueue, int index, BaseUnitEntity unit, Vector2 vector)
	{
		gameCommandQueue.AddCommand(new PartyFormationOffsetGameCommand(Game.Instance.Player.FormationManager.CurrentFormationIndex, index, unit, vector));
	}

	public static void TriggerLoot([NotNull] this GameCommandQueue gameCommandQueue, InteractionLootPart interactionLootPart, TriggerLootGameCommand.TriggerType type, [CanBeNull] ItemEntity item = null)
	{
		gameCommandQueue.AddCommand(new TriggerLootGameCommand(interactionLootPart, type, item));
	}

	public static void RepairShip([NotNull] this GameCommandQueue gameCommandQueue, int restoreHealth)
	{
		gameCommandQueue.AddCommand(new RepairShipGameCommand(restoreHealth));
	}

	public static void TestLoadingProcessCommandsLogicGameCommand([NotNull] this GameCommandQueue gameCommandQueue, int counter, NetPlayer player)
	{
		gameCommandQueue.AddCommand(new TestLoadingProcessCommandsLogicGameCommand(counter, player));
	}

	public static void StopUnits([NotNull] this GameCommandQueue gameCommandQueue, IList<BaseUnitEntity> units)
	{
		gameCommandQueue.AddCommand(new StopUnitsGameCommand(units));
	}

	public static void HoldUnits([NotNull] this GameCommandQueue gameCommandQueue, IList<BaseUnitEntity> units)
	{
		gameCommandQueue.AddCommand(new HoldUnitsGameCommand(units));
	}

	public static void VisitStarSystem([NotNull] this GameCommandQueue gameCommandQueue, SectorMapObjectEntity starSystem)
	{
		gameCommandQueue.AddCommand(new VisitStarSystemGameCommand(starSystem));
	}

	public static void SelectColony([NotNull] this GameCommandQueue gameCommandQueue, Colony colony)
	{
		bool isSynchronized = colony.StartedChronicles.Any();
		gameCommandQueue.AddCommand(new SelectColonyGameCommand(colony, isSynchronized));
	}

	public static void StartChronicleUI([NotNull] this GameCommandQueue gameCommandQueue, Colony colony)
	{
		if (colony != null && (colony.HasStartedChronicles || colony.HasFinishedProjectsSinceLastVisit))
		{
			bool hasStartedChronicles = colony.HasStartedChronicles;
			gameCommandQueue.AddCommand(new StartChronicleUIGameCommand(colony, hasStartedChronicles));
		}
	}

	public static void ClearFinishedProjectsSinceLastVisit([NotNull] this GameCommandQueue gameCommandQueue, ColonyRef colonyRef)
	{
		gameCommandQueue.AddCommand(new ClearFinishedProjectsSinceLastVisitGameCommand(colonyRef));
	}

	public static void ReceiveLootFromColony([NotNull] this GameCommandQueue gameCommandQueue, ColonyRef colonyRef)
	{
		gameCommandQueue.AddCommand(new ReceiveLootFromColonyGameCommand(colonyRef));
	}

	public static void UIEventTrigger([NotNull] this GameCommandQueue gameCommandQueue, UIEventTrigger uiEventTrigger)
	{
		gameCommandQueue.AddCommand(new UIEventTriggerGameCommand(uiEventTrigger));
	}

	public static void FinishRespec([NotNull] this GameCommandQueue gameCommandQueue, BaseUnitEntity respecEntity, bool forFree)
	{
		gameCommandQueue.AddCommand(new FinishRespecGameCommand(respecEntity, forFree));
	}

	public static void DrawMovePrediction([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BaseUnitEntity unit, [NotNull] Path path, [CanBeNull] float[] costPerEveryCell, [CanBeNull] UnitCommandParams unitCommandParams)
	{
		gameCommandQueue.AddCommand(new DrawMovePredictionGameCommand(unit, path, costPerEveryCell, unitCommandParams));
	}

	public static void ClearMovePrediction([NotNull] this GameCommandQueue gameCommandQueue)
	{
		bool isSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
		gameCommandQueue.AddCommand(new ClearMovePredictionGameCommand(isSynchronized));
	}

	public static void SetEquipmentColor([NotNull] this GameCommandQueue gameCommandQueue, BaseUnitEntity unit, RampColorPreset.IndexSet indexSet)
	{
		gameCommandQueue.AddCommand(new SetEquipmentColorGameCommand(indexSet, unit));
	}

	public static void SetEquipmentColor(this GameCommandQueue gameCommandQueue, BaseUnitEntity unit, Texture2D texture)
	{
		gameCommandQueue.AddCommand(new SetEquipmentColorGameCommand(unit, texture));
	}

	public static void GroupChanger([NotNull] this GameCommandQueue gameCommandQueue, UnitReference unitReference)
	{
		gameCommandQueue.AddCommand(new GroupChangerGameCommand(unitReference));
	}

	public static void CameraFollowTimeScale([NotNull] this GameCommandQueue gameCommandQueue, float scale, bool force)
	{
		gameCommandQueue.AddCommand(new CameraFollowTimeScaleGameCommand(scale, force));
	}

	public static void SetToCargoAutomatically([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintItem blueprint, bool toCargo)
	{
		bool isSynchronized = !ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current;
		gameCommandQueue.AddCommand(new SetToCargoAutomaticallyGameCommand(blueprint, toCargo, isSynchronized));
	}

	public static void ClearPointerMode([NotNull] this GameCommandQueue gameCommandQueue)
	{
		if (!ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current)
		{
			Game.Instance.ClickEventsController.ClearPointerMode();
		}
		else
		{
			gameCommandQueue.AddCommand(new ClearPointerModeGameCommand());
		}
	}

	public static void InterruptMoveUnit([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] AbstractUnitEntity unit)
	{
		gameCommandQueue.AddCommand(new InterruptMoveUnitGameCommand(unit));
	}
}
