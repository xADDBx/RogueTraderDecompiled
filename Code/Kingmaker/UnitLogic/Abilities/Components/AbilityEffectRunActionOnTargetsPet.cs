using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("ad6a2459189c4a339cb4250ea7ad10e1")]
public class AbilityEffectRunActionOnTargetsPet : AbilityApplyEffect
{
	public ActionList Actions;

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		PFLog.Default.Log("Apply ability effect to the pet of" + target);
		UnitPartPetOwner optional;
		if (target.Entity == null || (optional = target.Entity.GetOptional<UnitPartPetOwner>()) == null)
		{
			PFLog.Default.Error(context.AbilityBlueprint, "Target is not a pet owner");
			return;
		}
		using (context.GetDataScope(optional.PetUnit))
		{
			Actions.Run();
		}
	}

	public bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		if (target.Entity == null || target.Entity.GetOptional<UnitPartPetOwner>() == null)
		{
			return false;
		}
		if (Actions.Actions == null || Actions.Actions.Length == 0)
		{
			return true;
		}
		return Actions.Actions.All((GameAction a) => !(a is ContextAction contextAction) || contextAction.IsValidToCast(target, caster, casterPosition));
	}
}
