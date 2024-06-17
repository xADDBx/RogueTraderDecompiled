using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("675b13ddef90fff4491bc2e8cdee00fb")]
public class WarhammerContextActionStartGroupCooldown : ContextAction
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_AbilityGroup;

	public override string GetCaption()
	{
		return "Start cooldown for " + m_AbilityGroup.Get().name + " ability group";
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
			entity.GetAbilityCooldownsOptional()?.StartGroupCooldown(m_AbilityGroup);
		}
	}
}
