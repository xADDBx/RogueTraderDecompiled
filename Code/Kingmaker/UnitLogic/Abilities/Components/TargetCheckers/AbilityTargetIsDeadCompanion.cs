using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("cc4b8099545662d4396cbbbe993330b4")]
public class AbilityTargetIsDeadCompanion : BlueprintComponent, IAbilityTargetRestriction, ICanTargetDeadUnits
{
	public bool CanTargetDead => true;

	public bool CanTargetAlive => false;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper t, Vector3 casterPosition)
	{
		MechanicEntity entity = t.Entity;
		bool flag = entity != null;
		bool isPlayerFaction = entity.IsPlayerFaction;
		bool isDeadOrUnconscious = entity.IsDeadOrUnconscious;
		bool flag2 = entity.IsInCompanionRoster();
		return flag && isPlayerFaction && isDeadOrUnconscious && flag2;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetIsDeadCompanion;
	}
}
