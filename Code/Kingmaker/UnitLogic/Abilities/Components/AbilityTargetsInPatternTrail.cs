using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("ac023e5fb28349e2b8f0c2f50eea3d6a")]
public class AbilityTargetsInPatternTrail : AbilitySelectTarget, IAbilityAoEPatternProvider
{
	[SerializeField]
	private AbilityAoEPatternSettings m_PatternSettings;

	[SerializeField]
	private bool m_IncludeDead;

	[SerializeField]
	private bool m_IncludeCaster;

	[SerializeField]
	private bool m_IncludeNonUnitTargets;

	[SerializeField]
	private BlueprintProjectileReference m_Projectile;

	[SerializeField]
	private ConditionsChecker m_Condition;

	[SerializeField]
	private bool m_CheckNumberOfTargets;

	[SerializeField]
	[ShowIf("m_CheckNumberOfTargets")]
	private PropertyCalculator m_NumberOfTargets;

	public bool IsIgnoreLos => m_PatternSettings.IsIgnoreLos;

	public bool UseMeleeLos => m_PatternSettings.UseMeleeLos;

	public bool IsIgnoreLevelDifference => m_PatternSettings.IsIgnoreLevelDifference;

	public int PatternAngle => m_PatternSettings.PatternAngle;

	public bool CalculateAttackFromPatternCentre => m_PatternSettings.CalculateAttackFromPatternCentre;

	public bool ExcludeUnwalkable => false;

	public TargetType Targets => m_PatternSettings.Targets;

	public AoEPattern Pattern => m_PatternSettings.Pattern;

	public void OverridePattern(AoEPattern pattern)
	{
		m_PatternSettings.OverridePattern(pattern);
	}

	public OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		return m_PatternSettings.GetOrientedPattern(ability, casterNode, targetNode, coveredTargetsOnly);
	}

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		MechanicEntity caster = context.Caster;
		CustomGridNodeBase targetNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(anchor.Point).node;
		CustomGridNodeBase casterNode = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(caster.Position).node;
		AbilityAoEPatternSettings patternSettings = m_PatternSettings;
		PartAbilityPatternSettings abilityPatternSettingsOptional = caster.GetAbilityPatternSettingsOptional();
		_ = Pattern;
		if (abilityPatternSettingsOptional != null)
		{
			foreach (var item in abilityPatternSettingsOptional.m_OverrideRadius)
			{
				using (item.Runtime.RequestEventContext())
				{
					AoEPattern pattern2 = item.Component.OverrideRadius(context.Ability, Pattern);
					patternSettings.OverridePattern(pattern2);
				}
			}
		}
		OrientedPatternData pattern = GetOrientedPattern(context.Ability, casterNode, targetNode);
		PropertyContext context2 = new PropertyContext(caster, null, null, context, null, context.Ability);
		int numberOfTargets = (m_CheckNumberOfTargets ? m_NumberOfTargets.GetValue(context2) : int.MaxValue);
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			if ((!m_IncludeNonUnitTargets && !(mechanicEntity is BaseUnitEntity)) || (!m_IncludeDead && mechanicEntity != null && mechanicEntity.IsDeadOrUnconscious) || (mechanicEntity == context.Caster && !m_IncludeCaster) || !AoEPatternHelper.WouldTargetEntity(pattern, mechanicEntity))
			{
				continue;
			}
			if (Targets != TargetType.Any)
			{
				PartCombatGroup combatGroupOptional = mechanicEntity.GetCombatGroupOptional();
				switch (Targets)
				{
				case TargetType.Enemy:
					if (combatGroupOptional != null && !combatGroupOptional.IsEnemy(caster))
					{
						continue;
					}
					break;
				case TargetType.Ally:
					if (combatGroupOptional == null || !combatGroupOptional.IsAlly(caster))
					{
						continue;
					}
					break;
				}
			}
			if (m_Condition.HasConditions)
			{
				using (context.GetDataScope(mechanicEntity.ToITargetWrapper()))
				{
					if (!m_Condition.Check())
					{
						continue;
					}
				}
			}
			if (IsIgnoreLos || LosCalculations.GetWarhammerLos(context.Pattern.ApplicationNode.Vector3Position, context.Caster.SizeRect, mechanicEntity).CoverType != LosCalculations.CoverType.Invisible)
			{
				if (m_Projectile != null)
				{
					new ProjectileLauncher(m_Projectile.Get(), anchor, mechanicEntity).Ability(context.Ability).Launch();
				}
				yield return new TargetWrapper(mechanicEntity);
				numberOfTargets--;
				if (numberOfTargets <= 0)
				{
					break;
				}
			}
		}
	}
}
