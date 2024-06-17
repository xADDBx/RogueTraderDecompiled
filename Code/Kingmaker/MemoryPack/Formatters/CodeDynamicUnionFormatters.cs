using Kingmaker.GameCommands;
using Kingmaker.GameCommands.Cheats;
using Kingmaker.GameCommands.Colonization;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using MemoryPack;
using MemoryPack.Formatters;

namespace Kingmaker.MemoryPack.Formatters;

public static class CodeDynamicUnionFormatters
{
	public static void Register()
	{
		MemoryPackFormatterProvider.Register(new DynamicUnionFormatter<GameCommand>((0, typeof(AcceptChangeGroupGameCommand)), (1, typeof(AddCargoToSellGameCommand)), (2, typeof(AddForBuyVendorGameCommand)), (3, typeof(AreaTransitionGameCommand)), (4, typeof(AreaTransitionPartGameCommand)), (5, typeof(AttuneAbilityForPostGameCommand)), (6, typeof(CameraFollowTimeScaleGameCommand)), (7, typeof(ChangePlayerRoleGameCommand)), (8, typeof(CharGenChangeAppearancePageGameCommand)), (9, typeof(CharGenChangePhaseGameCommand)), (10, typeof(CharGenChangeVoiceGameCommand)), (11, typeof(CharGenCloseGameCommand)), (12, typeof(CharGenSelectCareerPathGameCommand)), (13, typeof(CharGenSelectItemGameCommand)), (14, typeof(CharGenSetBeardColorGameCommand)), (15, typeof(CharGenSetBeardGameCommand)), (16, typeof(CharGenSetEquipmentColorGameCommand)), (17, typeof(CharGenSetEyebrowsColorGameCommand)), (18, typeof(CharGenSetEyebrowsGameCommand)), (19, typeof(CharGenSetGenderGameCommand)), (20, typeof(CharGenSetHairColorGameCommand)), (21, typeof(CharGenSetHairGameCommand)), (22, typeof(CharGenSetHeadGameCommand)), (23, typeof(CharGenSetNameGameCommand)), (24, typeof(CharGenSetPortGameCommand)), (25, typeof(CharGenSetPortraitGameCommand)), (26, typeof(CharGenSetPregenGameCommand)), (27, typeof(CharGenSetRaceGameCommand)), (28, typeof(CharGenSetScarGameCommand)), (29, typeof(CharGenSetShipGameCommand)), (30, typeof(CharGenSetShipNameGameCommand)), (31, typeof(CharGenSetSkinColorGameCommand)), (32, typeof(CharGenSetTattooColorGameCommand)), (33, typeof(CharGenSetTattooGameCommand)), (34, typeof(CharGenSwitchPortraitTabGameCommand)), (35, typeof(CharGenTryAdvanceStatGameCommand)), (36, typeof(RunCheatCommandGameCommand)), (37, typeof(RunExternalCheatGameCommand)), (38, typeof(SetCheatVariableGameCommand)), (39, typeof(ClearAreaTransitionGroupGameCommand)), (40, typeof(ClearMovePredictionGameCommand)), (41, typeof(ClearPointerModeGameCommand)), (42, typeof(CloseChangeGroupGameCommand)), (43, typeof(CloseExplorationScreenCommand)), (44, typeof(CloseScreenCommand)), (45, typeof(CollectLootGameCommand)), (46, typeof(ColonyManagementUIGameCommand)), (47, typeof(ColonyProjectsUICloseGameCommand)), (48, typeof(ColonyProjectsUIOpenGameCommand)), (49, typeof(CompleteContractGameCommand)), (50, typeof(CreateColonyGameCommand)), (51, typeof(RemoveResourceMinerGameCommand)), (52, typeof(StartColonyEventGameCommand)), (53, typeof(StartColonyProjectGameCommand)), (54, typeof(UseResourceMinerGameCommand)), (55, typeof(CommitLevelUpGameCommand)), (56, typeof(CreateCargoInternalGameCommand)), (57, typeof(CreateNewWarpRouteGameCommand)), (58, typeof(DealSellCargoesGameCommand)), (59, typeof(DialogAnswerGameCommand)), (60, typeof(DowngradeSystemComponentGameCommand)), (61, typeof(DrawMovePredictionGameCommand)), (62, typeof(DropItemGameCommand)), (63, typeof(EndTradingGameCommand)), (64, typeof(EndTurnGameCommand)), (65, typeof(EquipItemGameCommand)), (66, typeof(ExitSpaceCombatGameCommand)), (67, typeof(FinishRespecGameCommand)), (68, typeof(GroupChangerGameCommand)), (69, typeof(HoldUnitsGameCommand)), (70, typeof(InteractWithStarSystemObjectGameCommand)), (71, typeof(LoadAreaGameCommand)), (72, typeof(LowerWarpRouteDifficultyGameCommand)), (73, typeof(MergeSlotGameCommand)), (74, typeof(MoveShipGameCommand)), (75, typeof(NotifySavingErrorCommand)), (76, typeof(PartyFormationIndexGameCommand)), (77, typeof(PartyFormationOffsetGameCommand)), (78, typeof(PartyFormationResetGameCommand)), (79, typeof(PingActionBarAbilityGameCommand)), (80, typeof(PingDialogAnswerGameCommand)), (81, typeof(PingDialogAnswerVoteGameCommand)), (82, typeof(PingEntityGameCommand)), (83, typeof(PingPositionGameCommand)), (84, typeof(PointOfInterestInteractGameCommand)), (85, typeof(PointOfInterestSetInteractedGameCommand)), (86, typeof(ReceiveLootFromColonyGameCommand)), (87, typeof(RemoveCargoFromSellGameCommand)), (88, typeof(RemoveFromBuyVendorGameCommand)), (89, typeof(RepairShipGameCommand)), (90, typeof(RequestPauseGameCommand)), (91, typeof(SaveGameCommand)), (92, typeof(ScanOnSectorMapGameCommand)), (93, typeof(ScanStarSystemObjectGameCommand)), (94, typeof(SelectColonyGameCommand)), (95, typeof(SetEquipmentColorGameCommand)), (96, typeof(SetInventorySorterGameCommand)), (97, typeof(SetPauseGameCommand)), (98, typeof(SetQuestObjectiveViewedGameCommand)), (99, typeof(SetQuestViewedGameCommand)), (100, typeof(SettingGameCommand)), (101, typeof(SetToCargoAutomaticallyGameCommand)), (102, typeof(SetUnitOnPostGameCommand)), (103, typeof(SignalGameCommand)), (104, typeof(SkipBarkGameCommand)), (105, typeof(SkipCutsceneGameCommand)), (106, typeof(SpeedUpGameCommand)), (107, typeof(SplitSlotGameCommand)), (108, typeof(StartTradingGameCommand)), (109, typeof(StartWarpTravelGameCommand)), (110, typeof(StopSpeedUpGameCommand)), (111, typeof(StopStarSystemStarShipGameCommand)), (112, typeof(StopUnitsGameCommand)), (113, typeof(SwapSlotsGameCommand)), (114, typeof(SwitchHandEquipmentGameCommand)), (115, typeof(SwitchPartyCharactersGameCommand)), (116, typeof(TestLoadingProcessCommandsLogicGameCommand)), (117, typeof(TransferItemGameCommand)), (118, typeof(TransferItemsToCargoGameCommand)), (119, typeof(TransferItemToInventoryGameCommand)), (120, typeof(TriggerLootGameCommand)), (121, typeof(UIEventTriggerGameCommand)), (122, typeof(UnequipItemGameCommand)), (123, typeof(UpgradeSystemComponentGameCommand)), (124, typeof(VisitStarSystemGameCommand))));
		MemoryPackFormatterProvider.Register(new DynamicUnionFormatter<GameCommandWithSynchronized>((0, typeof(AreaTransitionPartGameCommand)), (1, typeof(ClearMovePredictionGameCommand)), (2, typeof(CloseScreenCommand)), (3, typeof(CreateColonyGameCommand)), (4, typeof(SetToCargoAutomaticallyGameCommand)), (5, typeof(StartTradingGameCommand)), (6, typeof(StopStarSystemStarShipGameCommand)), (7, typeof(TriggerLootGameCommand))));
		MemoryPackFormatterProvider.Register(new DynamicUnionFormatter<UnitCommandParams>((0, typeof(UnitAreaTransitionParams)), (1, typeof(UnitGroupCommandParams)), (2, typeof(InterruptMoveUnitCommandParams)), (3, typeof(PlayerUseAbilityParams)), (4, typeof(UnitActivateAbilityParams)), (5, typeof(UnitAttackOfOpportunityParams)), (6, typeof(UnitDirectInteractParams)), (7, typeof(UnitDoNothingParams)), (8, typeof(UnitFollowParams)), (9, typeof(UnitInteractWithObjectParams)), (10, typeof(UnitInteractWithUnitParams)), (11, typeof(UnitJumpAsideDodgeParams)), (12, typeof(UnitLootUnitParams)), (13, typeof(UnitMoveAlongPathParams)), (14, typeof(UnitMoveContinuouslyParams)), (15, typeof(UnitMoveToParams)), (16, typeof(UnitMoveToProperParams)), (17, typeof(UnitOverwatchAttackParams)), (18, typeof(UnitTeleportParams)), (19, typeof(UnitUseAbilityParams))));
	}
}
