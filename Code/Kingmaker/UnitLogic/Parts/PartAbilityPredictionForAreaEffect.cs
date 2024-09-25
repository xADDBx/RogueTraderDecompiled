using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilityPredictionForAreaEffect : UnitPart, IHashable
{
	private readonly List<(EntityFactComponent Runtime, SpawnAreaEffectOnAbilityCast Component)> m_PatternEntries = new List<(EntityFactComponent, SpawnAreaEffectOnAbilityCast)>();

	public OrientedPatternData? GetAreaEffectPatternNotFromPatternCenter(AbilityData ability, TargetWrapper target, CustomGridNodeBase overrideCasterNode = null)
	{
		if (target == null)
		{
			return null;
		}
		MechanicEntity caster = ability.Caster;
		PartAbilityPredictionForAreaEffect partAbilityPredictionForAreaEffectOptional = caster.GetPartAbilityPredictionForAreaEffectOptional();
		if (partAbilityPredictionForAreaEffectOptional == null)
		{
			return null;
		}
		CustomGridNodeBase nearestNode = target.NearestNode;
		CustomGridNodeBase customGridNodeBase = overrideCasterNode ?? caster.GetNearestNodeXZ();
		CustomGridNodeBase innerNodeNearestToTarget = caster.GetInnerNodeNearestToTarget(customGridNodeBase, nearestNode.Vector3Position);
		CustomGridNodeBase outerNodeNearestToTarget = caster.GetOuterNodeNearestToTarget(customGridNodeBase, nearestNode.Vector3Position);
		IAbilityAoEPatternProvider abilityAoEPatternProvider = null;
		bool flag = false;
		foreach (var patternEntry in partAbilityPredictionForAreaEffectOptional.m_PatternEntries)
		{
			using (patternEntry.Runtime.RequestEventContext())
			{
				BlueprintAbilityAreaEffect blueprintAbilityAreaEffect = patternEntry.Component.GetBlueprintAbilityAreaEffect(ability, customGridNodeBase);
				if (blueprintAbilityAreaEffect != null)
				{
					abilityAoEPatternProvider = blueprintAbilityAreaEffect;
					flag = patternEntry.Component.GetOrientationFromCaster;
					break;
				}
			}
		}
		if (abilityAoEPatternProvider?.Pattern == null)
		{
			return null;
		}
		Vector3 vector = AoEPattern.GetCastDirection(abilityAoEPatternProvider.Pattern.Type, innerNodeNearestToTarget, nearestNode, nearestNode);
		bool flag2 = flag && vector.sqrMagnitude < 1f;
		if (flag2)
		{
			vector = caster.Forward;
		}
		using (ProfileScope.New("GetOriented from PartPredictionForAreaEffect"))
		{
			return abilityAoEPatternProvider.Pattern.GetOriented(innerNodeNearestToTarget, flag2 ? (outerNodeNearestToTarget.Vector3Position + vector * 1.Cells().Meters).GetNearestNodeXZUnwalkable() : outerNodeNearestToTarget, vector, abilityAoEPatternProvider.IsIgnoreLos, abilityAoEPatternProvider.IsIgnoreLevelDifference, isDirectional: true, coveredTargetsOnly: false, abilityAoEPatternProvider.UseMeleeLos);
		}
	}

	public void Add(SpawnAreaEffectOnAbilityCast component)
	{
		m_PatternEntries.Add((component.Runtime, component));
	}

	public void Remove(SpawnAreaEffectOnAbilityCast component)
	{
		m_PatternEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_PatternEntries.Empty())
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
