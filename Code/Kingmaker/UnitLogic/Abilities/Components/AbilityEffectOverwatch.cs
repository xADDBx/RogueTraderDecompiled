using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("0966a95df3bfa574e855a652cc54afa2")]
public class AbilityEffectOverwatch : AbilityApplyEffect
{
	public OverwatchMode Mode;

	public OverwatchHitsPerTarget HitsPerTarget;

	[SerializeField]
	[HideIf("UseFirstWeaponAbility")]
	private BlueprintAbilityReference m_AbilityOnTrigger;

	public bool UseFirstWeaponAbility;

	public BlueprintBuffReference[] ApplyingBuffs;

	public bool MultipleIfHasFact;

	[SerializeField]
	[ShowIf("MultipleIfHasFact")]
	private BlueprintUnitFactReference m_MultipleFact;

	public BlueprintAbility AbilityOnTrigger => m_AbilityOnTrigger.Get();

	public BlueprintUnitFact MultipleFact => m_MultipleFact?.Get();

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return;
		}
		AbilityData ability = context.Ability;
		if (ability == null)
		{
			PFLog.Default.Error("Ability for overwatch is missing");
			return;
		}
		Ability ability2 = ((!UseFirstWeaponAbility) ? maybeCaster.Facts.Get<Ability>(AbilityOnTrigger) : maybeCaster.GetFirstWeapon()?.Abilities.FirstOrDefault());
		if (ability2 == null)
		{
			PFLog.Default.ErrorWithReport("Ability for overwatch trigger is missing");
			return;
		}
		OverwatchHitsPerTarget hitsPerTarget = ((MultipleIfHasFact && maybeCaster.Facts.Contains(MultipleFact)) ? OverwatchHitsPerTarget.HitManyTimes : HitsPerTarget);
		maybeCaster.GetOrCreate<PartOverwatch>().Start(ability, ability2.Data, target, Mode, hitsPerTarget);
	}

	public static HashSet<CustomGridNodeBase> GetOverwatchArea(AbilityData overwatchAbility, TargetWrapper target)
	{
		MechanicEntity caster = overwatchAbility.Caster;
		if (caster is StarshipEntity)
		{
			return overwatchAbility.GetRestrictedFiringArcNodes();
		}
		IAbilityAoEPatternProvider patternSettings = overwatchAbility.GetPatternSettings();
		if (patternSettings == null)
		{
			return null;
		}
		CustomGridNodeBase casterNode = (CustomGridNodeBase)(GraphNode)caster.CurrentNode;
		CustomGridNodeBase nearestNodeXZUnwalkable = target.Point.GetNearestNodeXZUnwalkable();
		return patternSettings.GetOrientedPattern(overwatchAbility, casterNode, nearestNodeXZUnwalkable).Nodes.ToHashSet();
	}
}
