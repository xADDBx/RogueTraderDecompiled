using System;
using System.Collections.Generic;
using Kingmaker.AI.TargetSelectors;
using Kingmaker.AI.TargetSelectors.CustomSelectors;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.TargetSelectorHelpers;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.AI;

public class AbilityInfo : IAbilityDataProviderForPattern
{
	public int minRange;

	public int maxRange;

	public int effectiveRange;

	public bool isGrenadeTypeAOE;

	public int burstAttacksCount;

	public IAbilityAoEPatternProvider patternProvider;

	public AoEPattern pattern;

	public TargetType aoeIntendedTargets;

	public bool isCharge;

	public IntRect patternBounds;

	public AbilityData ability;

	public AbilitySettings settings;

	public ReadonlyList<RuleCalculateScatterShotHitDirectionProbability> scatterShotHitDirectionProbabilities;

	private Dictionary<(CustomGridNodeBase, TargetWrapper), (bool canTarget, int distance, LosCalculations.CoverType los)> m_CachedLOS = new Dictionary<(CustomGridNodeBase, TargetWrapper), (bool, int, LosCalculations.CoverType)>();

	private Dictionary<(UnitEntity, LosCalculations.CoverType), float> m_CachedDodgeChances = new Dictionary<(UnitEntity, LosCalculations.CoverType), float>();

	private Dictionary<(CustomGridNodeBase, CustomGridNodeBase), bool> m_CachedNodeLOS = new Dictionary<(CustomGridNodeBase, CustomGridNodeBase), bool>();

	public MechanicEntity Caster => ability.Caster;

	public int RangeCells => maxRange;

	public int BurstAttacksCount => burstAttacksCount;

	public bool IsScatter => ability.IsScatter;

	public ItemEntityStarshipWeapon StarshipWeapon => ability.StarshipWeapon;

	public ReadonlyList<RuleCalculateScatterShotHitDirectionProbability> ScatterShotHitDirectionProbabilities => scatterShotHitDirectionProbabilities;

	public AbilityData Data => ability;

	public AbilityInfo(AbilityData ability)
	{
		this.ability = ability;
		minRange = ability.MinRangeCells;
		maxRange = ability.RangeCells;
		effectiveRange = ability.Weapon?.AttackOptimalRange ?? ability.RangeCells;
		isGrenadeTypeAOE = false;
		burstAttacksCount = ability.BurstAttacksCount;
		patternProvider = ability.GetPatternSettings();
		pattern = patternProvider?.Pattern;
		aoeIntendedTargets = patternProvider?.Targets ?? TargetType.Enemy;
		isCharge = ability.IsCharge;
		settings = Caster.GetBrainOptional()?.Blueprint?.GetCustomAbilitySettings(ability.Blueprint);
		if (ability.Blueprint.CanTargetPointAfterRestrictions(ability) && pattern != null)
		{
			isGrenadeTypeAOE = true;
			patternBounds = pattern.Bounds;
			int num = Math.Max(Math.Max(patternBounds.xmax, patternBounds.ymax), Math.Max(-patternBounds.xmin, -patternBounds.ymin));
			effectiveRange = Math.Max(minRange, maxRange - num);
		}
		scatterShotHitDirectionProbabilities = ability.ScatterShotHitDirectionProbabilities;
	}

	public AbilityTargetSelector GetAbilityTargetSelector()
	{
		if (ability.Blueprint.TryGetComponent<AbilityTargetByDistanceSelectorHelper>(out var _))
		{
			return new SingleTargetSelectorByDistance(this);
		}
		if (ability.IsScatter)
		{
			return new ScatterShotTargetSelector(this);
		}
		if (isGrenadeTypeAOE || isCharge || ability.IsAOE)
		{
			return new AOETargetSelector(this);
		}
		return new SingleTargetSelector(this);
	}

	public bool CanTargetFromNodeCached(CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, TargetWrapper target, out int distance, out LosCalculations.CoverType los)
	{
		if (m_CachedLOS.TryGetValue((casterNode, target), out (bool, int, LosCalculations.CoverType) value))
		{
			distance = value.Item2;
			los = value.Item3;
			return value.Item1;
		}
		bool flag = ability.CanTargetFromNode(casterNode, targetNode, target, out distance, out los);
		m_CachedLOS[(casterNode, target)] = (flag, distance, los);
		return flag;
	}

	public float CalculateDodgeChanceCached(UnitEntity unit, LosCalculations.CoverType coverType)
	{
		if (m_CachedDodgeChances.TryGetValue((unit, coverType), out var value))
		{
			return value;
		}
		float num = ability.CalculateDodgeChanceCached(unit, coverType);
		m_CachedDodgeChances[(unit, coverType)] = num;
		return num;
	}

	public bool HasLosCached(CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		if (m_CachedNodeLOS.TryGetValue((fromNode, toNode), out var value))
		{
			return value;
		}
		value = LosCalculations.HasLos(fromNode, default(IntRect), toNode, default(IntRect));
		m_CachedNodeLOS[(fromNode, toNode)] = value;
		return value;
	}

	public CustomGridNodeBase GetBestShootingPosition(CustomGridNodeBase castNode, TargetWrapper target)
	{
		return ability.GetBestShootingPosition(castNode, target);
	}
}
