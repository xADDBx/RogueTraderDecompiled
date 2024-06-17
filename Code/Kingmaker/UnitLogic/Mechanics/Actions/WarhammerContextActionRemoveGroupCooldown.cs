using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("a5d7b924bf7743f483c173754bdc072a")]
public class WarhammerContextActionRemoveGroupCooldown : ContextAction
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public BlueprintAbilityGroup AbilityGroup => m_AbilityGroup.Get();

	public override string GetCaption()
	{
		return "Reset cooldown for " + m_AbilityGroup.Get().name + " ability group";
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
			entity.GetAbilityCooldownsOptional()?.RemoveGroupCooldown(AbilityGroup);
		}
	}
}
