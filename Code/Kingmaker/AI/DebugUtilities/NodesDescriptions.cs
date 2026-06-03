namespace Kingmaker.AI.DebugUtilities;

public static class NodesDescriptions
{
	public const string AbilityIsNull = "Ability == null";

	public const string AbilityNotNull = "Ability != null";

	public const string UnitBrainIsHoldingPosition = "Unit.Brain.IsHoldingPosition";

	public const string UnitBrainIsNotUsualMeleeUnit = "!Unit.Brain.IsUsualMeleeUnit";

	public const string IsMovementInfluentAbility = "IsMovementInfluentAbility";

	public const string UnitCommandsEmptyAndUnitStateCanActInTurnBased = "Unit.Commands.Empty && Unit.State.CanActInTurnBased";

	public const string UnitLuredToSomeone = "Unit.GetOptional<UnitPartLure>()?.UnitLuredTo != null";

	public const string FirstScoreOrderIsScoreTypeBodyGuardScore = "ScoreOrder?.Order?.First() == ScoreType.BodyGuardScore";

	public const string UnitBrainResponseToAoOThreatAfterAbilityAndUnitCombatStateIsEngaged = "Unit.Brain.ResponseToAoOThreatAfterAbility && Unit.CombatState.IsEngaged";

	public const string UnitIsInSquad = "Unit.IsInSquad";

	public const string AbilityNotNullAndAbilityTargetNotNullAndAbilityCanTargetAndAbilityTargetSelectorIsNotScatterShotRisky = "Ability != null && AbilityTarget != null && Ability.CanTarget && !Ability.TargetSelector.IsScatterShotRisky";

	public const string UnitBrainResponseToAoOThreatAndUnitCombatStateIsEngaged = "Unit.Brain.ResponseToAoOThreat && Unit.CombatState.IsEngaged";

	public const string UnitBrainBlueprintTargetOthersIfCantReachHatedOrFalse = "Unit.Brain?.Blueprint?.TargetOthersIfCantReachHated ?? false";

	public const string UnitIsSquadLeader = "Unit == SquadLeader";

	public const string SquadLeaderTargetIsNotNull = "SquadLeaderTarget != null";

	public const string CurrentlyLoadedAreaTimeSurvivalNotNullAndCurrentlyLoadedAreaTimeSurvivalIsUnitShouldDoNothing = "CurrentlyLoadedArea.TimeSurvival != null && CurrentlyLoadedArea.TimeSurvival.IsShouldDoNothing(Unit)";

	public const string BestTrajectoryScoreLessUnitBrainTrajectoryScoreMinThreshold = "BestTrajectoryScore < Unit.Brain.TrajectoryScoreMinThreshold";

	public const string IsNotLastActionBrokePlan = "!IsLastActionBrokePlan";

	public const string AbilityTargetNotNullAndUnitBrainEnemyConditionsNotDirty = "AbilityTarget != null && !Unit.Brain.EnemyConditionsDirty";

	public const string CurrentSquadUnitIsGovernor = "CurrentSquadUnit is Governor";

	public const string IterateAllSquadUnits = "Iterate all squad units";

	public const string IterateAllSquadUnitsExceptLeader = "Iterate all squad units except leader";

	public const string WhileBestPathCountMore0AndNoFailure = "While BestPath.Count > 0 && No Failure";

	public const string IterateAllSquadUnitsExceptLeadingGolem = "Iterate all squad units except leading golem";

	public const string ConsideringAbilityNull = "ConsideringAbility = null";

	public const string AbilityConsideringAbility = "Ability = ConsideringAbility";

	public const string AbilityTargetUnitSquadCommonTarget = "AbilityTarget = Unit.GetSquadOptional().Squad.CommonTarget";

	public const string StoreMoveCommandForCurrentSquadUnit = "Store move command for current squad unit";

	public const string SpendAllMovePointsOfCurrentSquadUnit = "Spend all move points of current squad unit";

	public const string SetAbilityTargetToUnit = "Set ability target to unit";

	public const string StorePathToClosestEnemyForCurrentSquadUnit = "Store path to closest enemy for current squad unit";

	public const string SpendAllMovePointsOfCurrentUnit = "Spend all move points of current unit";

	public const string LeadingGolemClosestGolemToSquadLeader = "leadingGolem = closest golem to Squad Leader";

	public const string MakePreparationsForLeadingGolem = "Make preparations for leading golem";

	public const string PrepareSquadForMovementCalculations = "Prepare squad for movement calculations";

	public const string SetupGovernorMoveCommand = "Setup Governor move command";

	public const string SelectTargetForAbilityOrBodyguard = "Select target for ability or bodyguard";

	public const string SelectTargetForAbility = "Select target for ability";

	public const string FailIfIsLastActionBrokePlanOrUnitBrainEnemyConditionsDirty = "Fail if IsLastActionBrokePlan or Unit.Brain.EnemyConditionsDirty";
}
