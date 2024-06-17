using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("0bd97a65cf824c9db33fe4d1538535df")]
public class WarhammerContextActionRemoveAbilityCooldown : ContextAction
{
	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	public BlueprintAbility Ability => m_Ability.Get();

	public override string GetCaption()
	{
		return "Reset cooldown for " + m_Ability.Get().name + " ability";
	}

	public override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity == null)
		{
			PFLog.Default.Error("Target is missing");
		}
		else
		{
			entity.GetAbilityCooldownsOptional()?.RemoveAbilityCooldown(Ability);
		}
	}
}
