using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AI.AreaScanning;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.AI.Blueprints;
using Kingmaker.AI.Blueprints.Components;
using Kingmaker.AI.Scenarios;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.AI;

public class DecisionContext
{
	public struct BetterPlace
	{
		public AiAreaScanner.PathData PathData;

		public WarhammerPathAiCell BestCell;
	}

	protected bool m_AlliesInitialized;

	protected bool m_EnemiesInitialized;

	protected BlueprintFaction m_HatedFaction;

	protected readonly List<TargetInfo> m_TargetsFromPriorityScenario = new List<TargetInfo>();

	protected readonly List<TargetInfo> m_HatedTargets = new List<TargetInfo>();

	protected readonly List<TargetInfo> m_LowPriorityTargets = new List<TargetInfo>();

	protected readonly List<TargetInfo> m_Enemies = new List<TargetInfo>();

	protected readonly List<TargetInfo> m_Allies = new List<TargetInfo>();

	public readonly List<TargetInfo> Self = new List<TargetInfo>();

	public bool IsMoveCommand;

	[NotNull]
	private BaseUnitEntity m_Unit;

	private List<BlueprintAbility> PriorityAbilities;

	private static ScoreOrder DefaultScoreOrder = new ScoreOrder();

	public MechanicEntity LuredTo;

	public AiAreaScanner.PathData UnitMoveVariants;

	public BetterPlace FoundBetterPlace;

	public List<GraphNode> HoldPositionNodes = new List<GraphNode>();

	public SquadPhase SquadPhase;

	public BaseUnitEntity CurrentSquadUnit;

	public List<(BaseUnitEntity unit, UnitMoveToProperParams cmd)> SquadUnitsMoveCommands = new List<(BaseUnitEntity, UnitMoveToProperParams)>();

	private IEnumerator<BaseUnitEntity> SquadUnitsEnumerator;

	private IEnumerator<AbilityData> AbilitiesEnumerator;

	public AbilityData ConsideringAbility;

	private AbilityInfo m_AbilityInfo;

	public TargetWrapper AbilityTarget;

	public bool IsMovementInfluentAbility;

	private readonly Dictionary<CustomGridNodeBase, AiBrainHelper.ThreatsInfo> threatsCache = new Dictionary<CustomGridNodeBase, AiBrainHelper.ThreatsInfo>();

	public BaseUnitEntity Unit
	{
		get
		{
			if (SquadPhase == SquadPhase.None)
			{
				return m_Unit;
			}
			return CurrentSquadUnit;
		}
	}

	public CustomGridNodeBase UnitNode => Self[0].Node;

	public ScoreOrder ScoreOrder => Unit.Brain.ScoreOrder ?? DefaultScoreOrder;

	public bool IsLured => LuredTo != null;

	public UnitMoveToProperParams MoveCommand { get; set; }

	public BaseUnitEntity SquadLeader { get; private set; }

	public CustomGridNodeBase SquadLeaderNode => ((CustomGridNodeBase)(from x in SquadUnitsMoveCommands
		where x.unit == SquadLeader
		select x.cmd.ForcedPath.path.Last()).FirstOrDefault()) ?? SquadLeader.GetNearestNodeXZ();

	public MechanicEntity SquadLeaderTarget => Unit.GetSquadOptional()?.Squad.CommonTarget?.Entity;

	public bool IsPlayerFaction { get; private set; }

	public AbilityInfo AbilityInfo
	{
		get
		{
			AbilityData abilityData = ConsideringAbility ?? Ability;
			if (m_AbilityInfo == null || m_AbilityInfo.ability != abilityData)
			{
				if (abilityData == null)
				{
					m_AbilityInfo = null;
				}
				else
				{
					m_AbilityInfo = new AbilityInfo(abilityData);
				}
			}
			return m_AbilityInfo;
		}
	}

	public bool ShouldResponseToAoOThreat
	{
		get
		{
			if (Unit.Brain.ResponseToAoOThreat)
			{
				return Unit.CombatState.IsEngaged;
			}
			return false;
		}
	}

	public bool ShouldResponseToAoOThreatAfterAbility
	{
		get
		{
			if (Unit.Brain.ResponseToAoOThreatAfterAbility)
			{
				return Unit.CombatState.IsEngaged;
			}
			return false;
		}
	}

	[CanBeNull]
	public AbilityData Ability { get; set; }

	public List<TargetInfo> HatedTargets
	{
		get
		{
			if (!m_EnemiesInitialized)
			{
				InitializeEnemies();
			}
			if (Unit.Brain.EnemyConditionsDirty)
			{
				UpdateTargetsPriority();
			}
			PriorityTargetScenario priorityTargetScenario = Unit.Brain.CurrentScenario as PriorityTargetScenario;
			if (IsMoveCommand && priorityTargetScenario != null)
			{
				return m_TargetsFromPriorityScenario;
			}
			if (!IsMoveCommand && priorityTargetScenario != null)
			{
				if (priorityTargetScenario.AttackPriorityTargetsOnly)
				{
					return m_TargetsFromPriorityScenario;
				}
				return m_TargetsFromPriorityScenario.Concat(m_HatedTargets).ToList();
			}
			if (m_HatedTargets.Count <= 0)
			{
				if (m_Enemies.Count <= 0)
				{
					return m_LowPriorityTargets;
				}
				return m_Enemies;
			}
			return m_HatedTargets;
		}
	}

	public BlueprintFaction HatedFaction
	{
		get
		{
			if (!m_EnemiesInitialized)
			{
				InitializeEnemies();
			}
			if (Unit.Brain.EnemyConditionsDirty)
			{
				UpdateTargetsPriority();
			}
			return m_HatedFaction;
		}
	}

	public List<TargetInfo> Enemies
	{
		get
		{
			if (!m_EnemiesInitialized)
			{
				InitializeEnemies();
			}
			if (Unit.Brain.EnemyConditionsDirty)
			{
				UpdateTargetsPriority();
			}
			return m_Enemies;
		}
	}

	public List<TargetInfo> Allies
	{
		get
		{
			if (!m_AlliesInitialized)
			{
				Game.Instance.TurnController.AllUnits.Where((MechanicEntity u) => u.IsInCombat && u != Unit && Unit.IsAlly(u)).ForEach(delegate(MechanicEntity u)
				{
					m_Allies.Add(CreateTargetInfo(u));
				});
				m_AlliesInitialized = true;
			}
			return m_Allies;
		}
	}

	public IEnumerable<TargetInfo> Group => m_Allies.Where((TargetInfo u) => u.Entity.GetCombatGroupOptional() == Unit.CombatGroup);

	public void InitSquadUnitsEnumerator()
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		SquadUnitsEnumerator = ((currentUnit is UnitSquad squad) ? squad.GetConsciousUnits().GetEnumerator() : null);
	}

	public void ConsiderNextSquadUnit()
	{
		IEnumerator<BaseUnitEntity> squadUnitsEnumerator = SquadUnitsEnumerator;
		CurrentSquadUnit = ((squadUnitsEnumerator != null && squadUnitsEnumerator.MoveNext()) ? SquadUnitsEnumerator.Current : null);
	}

	public void InitAbilitiesEnumerator()
	{
		AbilitiesEnumerator = GetSortedAbilityList()?.GetEnumerator();
	}

	public void ConsiderNextAbility()
	{
		IEnumerator<AbilityData> abilitiesEnumerator = AbilitiesEnumerator;
		if (abilitiesEnumerator != null && abilitiesEnumerator.MoveNext())
		{
			ConsideringAbility = AbilitiesEnumerator.Current;
		}
		else
		{
			ConsideringAbility = null;
		}
	}

	public bool IsAbilityIgnoresAoO(AbilityData ability)
	{
		if (!(ability == null))
		{
			return ability.UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.CanUseWithoutAOO;
		}
		return true;
	}

	private void InitializeEnemies()
	{
		ReleaseTargets(m_Enemies);
		ReleaseTargets(m_HatedTargets);
		if (Unit is StarshipEntity starshipEntity)
		{
			m_HatedFaction = starshipEntity.Facts.GetComponents<StarshipAiHatedFaction>().FirstOrDefault()?.HatedFaction;
		}
		if (Unit.Brain.IsTraitor)
		{
			foreach (TargetInfo ally in Allies)
			{
				MechanicEntity entity = ally.Entity;
				if ((entity == null || entity.IsConscious) && !AbstractUnitCommand.CommandTargetUntargetable(Unit, ally.Entity))
				{
					m_Enemies.AddUnique(ally);
				}
			}
			m_Allies.Clear();
			m_HatedTargets.AddRange(Unit.Brain.GetHatedTargets(m_Enemies));
			m_EnemiesInitialized = true;
			return;
		}
		if (Unit.Brain.IsHuntingDownPriorityTarget)
		{
			PriorityTargetScenario priorityTargetScenario = Unit.Brain.CurrentScenario as PriorityTargetScenario;
			if (priorityTargetScenario.SimultaneousTargets)
			{
				m_TargetsFromPriorityScenario.AddRange(priorityTargetScenario.AllNotDestroyedTarget());
			}
			else
			{
				TargetInfo targetInfo = priorityTargetScenario.CurrentTarget();
				if (targetInfo != null)
				{
					m_TargetsFromPriorityScenario.Add(targetInfo);
				}
			}
		}
		foreach (EntityRef<MechanicEntity> customLowPriorityTarget in Unit.Brain.CustomLowPriorityTargets)
		{
			m_LowPriorityTargets.Add(CreateTargetInfo(customLowPriorityTarget));
		}
		foreach (UnitGroupMemory.UnitInfo enemyInfo in Unit.CombatGroup.Memory.Enemies)
		{
			BaseUnitEntity unit = enemyInfo.Unit;
			if (unit == null || m_TargetsFromPriorityScenario.HasItem((TargetInfo info) => info.Entity == enemyInfo.Unit) || m_LowPriorityTargets.HasItem((TargetInfo info) => info.Entity == enemyInfo.Unit))
			{
				continue;
			}
			PartLifeState lifeStateOptional = unit.GetLifeStateOptional();
			if ((lifeStateOptional != null && !lifeStateOptional.IsConscious) || AbstractUnitCommand.CommandTargetUntargetable(Unit, unit) || (Unit.Faction.IsPlayer && !unit.LifeState.IsUnconscious && !unit.CombatGroup.Memory.HasPlayerCharacterInMemory()))
			{
				continue;
			}
			if (unit.Faction.IsPlayer)
			{
				UnitAggroFilter unitAggroFilter = BlueprintComponentExtendAsObject.Or(Unit.Blueprint.GetComponent<UnitAggroFilter>(), null);
				if (unitAggroFilter != null && unitAggroFilter.ShouldAggro(Unit, unit))
				{
					continue;
				}
			}
			m_Enemies.Add(CreateTargetInfo(unit));
			Unit.Brain.EnemyConditionsDirty = false;
		}
		MechanicEntity manualTarget = Unit.CombatState.ManualTarget;
		if (Unit.Faction.IsPlayer && manualTarget != null && !manualTarget.IsDead && !AbstractUnitCommand.CommandTargetUntargetable(Unit, manualTarget))
		{
			bool flag = false;
			foreach (TargetInfo enemy in m_Enemies)
			{
				if (enemy.Entity == manualTarget)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_Enemies.Add(CreateTargetInfo(manualTarget));
			}
		}
		m_HatedTargets.AddRange(Unit.Brain.GetHatedTargets(m_Enemies));
		m_EnemiesInitialized = true;
	}

	public void UpdateTargetsPriority()
	{
		if (Unit is StarshipEntity starshipEntity)
		{
			m_HatedFaction = starshipEntity.Facts.GetComponents<StarshipAiHatedFaction>().FirstOrDefault()?.HatedFaction;
		}
		m_HatedTargets.Clear();
		m_HatedTargets.AddRange(Unit.Brain.GetHatedTargets(m_Enemies));
		m_Enemies.AddRange(m_LowPriorityTargets);
		m_LowPriorityTargets.Clear();
		foreach (EntityRef<MechanicEntity> target in Unit.Brain.CustomLowPriorityTargets)
		{
			TargetInfo targetInfo = m_Enemies.FirstOrDefault((TargetInfo unit) => unit.Entity == target);
			if (targetInfo == null)
			{
				m_LowPriorityTargets.Add(CreateTargetInfo(target));
				continue;
			}
			m_LowPriorityTargets.Add(targetInfo);
			m_Enemies.Remove(targetInfo);
		}
		Unit.Brain.EnemyConditionsDirty = false;
	}

	public void InitCurrentTurnEntity()
	{
		MechanicEntity currentUnit = Game.Instance.TurnController.CurrentUnit;
		if (currentUnit is BaseUnitEntity unit)
		{
			InitUnit(unit);
		}
		if (currentUnit is UnitSquad squad)
		{
			SquadPhase = SquadPhase.Move;
			SquadLeader = squad.SelectLeader();
			CurrentSquadUnit = SquadLeader;
			InitUnit(SquadLeader);
		}
	}

	public void InitUnit([NotNull] BaseUnitEntity unit)
	{
		m_Unit = unit;
		LuredTo = Unit.GetOptional<UnitPartLure>()?.UnitLuredTo;
		Ability = null;
		AbilityTarget = null;
		PriorityAbilities = GetPriorityAbilitiesSorted(Unit);
		if (Unit.Brain.IsHoldingPosition)
		{
			HoldPositionNodes.AddRange(((HoldPositionScenario)Unit.Brain.CurrentScenario).GetHoldPositionNodes());
		}
		Self.Add(CreateTargetInfo(unit));
		IsPlayerFaction = unit.Faction.IsPlayer;
	}

	public void ReleaseUnit()
	{
		HoldPositionNodes.Clear();
		ReleaseTargets(Self);
		ReleaseTargets(m_TargetsFromPriorityScenario);
		ReleaseTargets(m_LowPriorityTargets);
		ReleaseTargets(m_Enemies);
		m_HatedTargets.Clear();
		m_EnemiesInitialized = false;
		ReleaseTargets(m_Allies);
		m_AlliesInitialized = false;
		m_Unit = null;
		Ability = null;
		IsPlayerFaction = false;
	}

	protected TargetInfo CreateTargetInfo(MechanicEntity enemy)
	{
		return TargetInfo.Claim<TargetInfo>(enemy);
	}

	private static void ReleaseTargets(List<TargetInfo> targets)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			targets[i].Release();
		}
		targets.Clear();
	}

	public List<BlueprintAbility> GetPriorityAbilitiesSorted(BaseUnitEntity unit)
	{
		List<BlueprintAbility> list = new List<BlueprintAbility>();
		AbilitySourceWrapper[] priorityAbilities = unit.Brain.PriorityAbilities;
		if (priorityAbilities != null && priorityAbilities.Length != 0)
		{
			AbilitySourceWrapper[] array = priorityAbilities;
			foreach (AbilitySourceWrapper abilitySourceWrapper in array)
			{
				list.AddRange(abilitySourceWrapper.GetSorted(unit));
			}
		}
		return list;
	}

	public AbilityData GetBestAbility()
	{
		return GetSortedAbilityList()?.First();
	}

	public List<AbilityData> GetSortedAbilityList(CastTimepointType castTimepoint = CastTimepointType.None)
	{
		if (Unit == null)
		{
			return null;
		}
		List<AbilityData> usable = new List<AbilityData>();
		Unit.Abilities.RawFacts.ForEach(delegate(Ability ab)
		{
			if (IsUsableAbility(ab) && HasProperCastTimepoint(ab, castTimepoint))
			{
				usable.Add(ab.Data);
			}
		});
		if (usable.Count == 0)
		{
			return null;
		}
		usable.Sort(AbilitiesPriorityComparison);
		return usable;
	}

	private bool IsFirstAbilityBetter(AbilityData first, AbilityData second)
	{
		if (Unit.CombatState.IsEngaged && first.RangeCells <= 1 && second.RangeCells > 1)
		{
			return true;
		}
		if (first.Blueprint.GetComponent<WarhammerEndTurn>() == null && second.Blueprint.GetComponent<WarhammerEndTurn>() != null)
		{
			return true;
		}
		if (!Unit.IsInSquad && first.GetPatternSettings() != null && second.GetPatternSettings() == null)
		{
			return true;
		}
		if (Unit.IsInSquad && first.GetPatternSettings() == null && second.GetPatternSettings() != null)
		{
			return true;
		}
		if (first.Blueprint.Range != 0 && second.Blueprint.Range == AbilityRange.Personal)
		{
			return true;
		}
		return false;
	}

	public bool IsUsableAbility(Ability ability)
	{
		if (IsRestrictedBySettings(ability) || IsEscapeFromThreatAbility(ability))
		{
			return false;
		}
		AbilityData data = ability.Data;
		if (!data.IsAvailable)
		{
			List<AbilityData.UnavailabilityReasonType> unavailabilityReasons = data.GetUnavailabilityReasons();
			if (unavailabilityReasons.Count == 0)
			{
				return false;
			}
			return unavailabilityReasons.All((AbilityData.UnavailabilityReasonType x) => IsEnabledByMoveReason(x));
		}
		if (!Unit.Brain.IgnoreAoOThreatOnCast)
		{
			return IsSafeToCast(data);
		}
		return true;
	}

	private bool IsEnabledByMoveReason(AbilityData.UnavailabilityReasonType reason)
	{
		if (reason != AbilityData.UnavailabilityReasonType.CannotUseInThreatenedArea && reason != AbilityData.UnavailabilityReasonType.CannotUseInConcussionArea && reason != AbilityData.UnavailabilityReasonType.CannotUseInCantAttackArea)
		{
			return reason == AbilityData.UnavailabilityReasonType.CannotUseInInertWarpArea;
		}
		return true;
	}

	private bool IsRestrictedBySettings(Ability ability)
	{
		AbilitySettings abilitySettings = Unit.Brain.Blueprint?.GetCustomAbilitySettings(ability.Blueprint);
		if (abilitySettings == null)
		{
			return false;
		}
		if (abilitySettings.CantCastUntilRound <= Game.Instance.TurnController.CombatRound)
		{
			return IsRestrictedByDifficultySettings(abilitySettings.MinDifficultySetting);
		}
		return true;
	}

	private static bool IsRestrictedByDifficultySettings(GameDifficultyOption minAllowedDifficulty)
	{
		if (minAllowedDifficulty == GameDifficultyOption.Story)
		{
			return false;
		}
		return SettingsController.Instance.DifficultyPresetsController.CurrentDifficultyCompareTo(minAllowedDifficulty) < 0;
	}

	private bool IsSafeToCast(AbilityData ability)
	{
		if (ability.Caster.GetCombatStateOptional().IsEngaged)
		{
			return ability.UsingInThreateningArea == BlueprintAbility.UsingInThreateningAreaType.CanUseWithoutAOO;
		}
		return true;
	}

	private bool IsEscapeFromThreatAbility(Ability ability)
	{
		return ability.Blueprint.GetComponent<AiEscapeFromThreat>() != null;
	}

	private bool HasProperCastTimepoint(Ability ability, CastTimepointType castTimepoint)
	{
		BlueprintBrain obj = Unit.Brain?.Blueprint as BlueprintBrain;
		AbilitySourceWrapper[] source = obj?.BeforeMoveAbilities ?? Array.Empty<AbilitySourceWrapper>();
		AbilitySourceWrapper[] source2 = obj?.MoveAndCastAbilities ?? Array.Empty<AbilitySourceWrapper>();
		AbilitySourceWrapper[] source3 = obj?.AfterMoveAbilities ?? Array.Empty<AbilitySourceWrapper>();
		switch (castTimepoint)
		{
		case CastTimepointType.None:
			if (!source2.Contains((AbilitySourceWrapper w) => w.Abilities.Contains(ability.Blueprint)))
			{
				if (!source.Contains((AbilitySourceWrapper w) => w.Abilities.Contains(ability.Blueprint)))
				{
					return !source3.Contains((AbilitySourceWrapper w) => w.Abilities.Contains(ability.Blueprint));
				}
				return false;
			}
			return true;
		case CastTimepointType.BeforeMove:
			return source.Contains((AbilitySourceWrapper w) => w.Abilities.Contains(ability.Blueprint));
		case CastTimepointType.AfterMove:
			return source3.Contains((AbilitySourceWrapper w) => w.Abilities.Contains(ability.Blueprint));
		default:
			return true;
		}
	}

	public List<AbilityData> GetSortedMovementInfluentAbilities()
	{
		List<AbilityData> list = new List<AbilityData>();
		if (Unit == null)
		{
			return list;
		}
		AbilitySourceWrapper[] movementInfluentAbilities = Unit.Brain.MovementInfluentAbilities;
		if (movementInfluentAbilities == null || movementInfluentAbilities.Length == 0)
		{
			return list;
		}
		AbilitySourceWrapper[] array = movementInfluentAbilities;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (BlueprintAbility ability2 in array[i].Abilities)
			{
				Ability ability = Unit.Abilities.GetAbility(ability2);
				if (ability != null && IsUsableAbility(ability))
				{
					list.Add(ability.Data);
				}
			}
		}
		list.Sort(AbilitiesPriorityComparison);
		return list;
	}

	private int AbilitiesPriorityComparison(AbilityData a, AbilityData b)
	{
		int num = PriorityAbilities.FindIndex((BlueprintAbility u) => u == a.Blueprint);
		int num2 = PriorityAbilities.FindIndex((BlueprintAbility u) => u == b.Blueprint);
		if (num >= 0 || num2 >= 0)
		{
			if (num >= 0)
			{
				if (num2 >= 0)
				{
					return num - num2;
				}
				return -1;
			}
			return 1;
		}
		if (IsFirstAbilityBetter(a, b))
		{
			return -1;
		}
		if (IsFirstAbilityBetter(b, a))
		{
			return 1;
		}
		return b.CalculateActionPointCost() - a.CalculateActionPointCost();
	}

	public List<TargetInfo> GetAvailableTargets(AbilityData ability)
	{
		List<TargetInfo> list = TempList.Get<TargetInfo>();
		if (ability == null)
		{
			return list;
		}
		IAbilityAoEPatternProvider patternSettings = ability.GetPatternSettings();
		if (patternSettings != null)
		{
			switch (patternSettings.Targets)
			{
			case TargetType.Enemy:
				list.AddRange(HatedTargets);
				break;
			case TargetType.Ally:
				list.AddRange(Allies);
				list.AddRange(Self);
				break;
			case TargetType.Any:
				list.AddRange(HatedTargets);
				list.AddRange(Allies);
				list.AddRange(Self);
				break;
			}
		}
		else if (ability.Blueprint.CanTargetEnemies)
		{
			list.AddRange(HatedTargets);
		}
		else
		{
			if (ability.Blueprint.CanTargetFriends && !ability.IsScatter)
			{
				list.AddRange(Allies);
			}
			if (ability.Blueprint.CanTargetSelf && !ability.IsScatter)
			{
				list.AddRange(Self);
			}
		}
		return list;
	}

	public float GetTargetPriority(MechanicEntity target)
	{
		if (Ability == null)
		{
			return 0f;
		}
		return Unit.Brain.GetTargetPriority(Ability, target);
	}

	public IEnumerable<MechanicEntity> GetEngagingEnemies()
	{
		foreach (TargetInfo enemy in Enemies)
		{
			BaseUnitEntity baseUnitEntity = (BaseUnitEntity)enemy.Entity;
			if (baseUnitEntity.GetThreatHand()?.Weapon.Blueprint.AttackOfOpportunityAbility != null)
			{
				yield return baseUnitEntity;
			}
		}
	}

	public HashSet<GraphNode> GetEngagedNodes()
	{
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		foreach (BaseUnitEntity engagingEnemy in GetEngagingEnemies())
		{
			hashSet.UnionWith(engagingEnemy.GetThreateningArea());
		}
		return hashSet;
	}

	public AiBrainHelper.ThreatsInfo FindThreats(BaseUnitEntity unit, CustomGridNodeBase node)
	{
		if (threatsCache.TryGetValue(node, out var value))
		{
			return value;
		}
		value = AiBrainHelper.TryFindThreats(unit, node);
		threatsCache[node] = value;
		return value;
	}
}
