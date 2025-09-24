using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("66e032e5cf38801428940a1a0d14b946")]
public class AbilityEffectRunAction : AbilityApplyEffect
{
	public SavingThrowType SavingThrowType;

	public ActionList Actions;

	public bool HasSavingThrow => SavingThrowType != SavingThrowType.Unknown;

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		PFLog.Default.Log("Apply ability effect to " + target);
		RulePerformSavingThrow rule = null;
		if (HasSavingThrow)
		{
			if (target.Entity == null)
			{
				PFLog.Default.Error(context.AbilityBlueprint, "Can't roll SavingThrow because target is not an unit");
				return;
			}
			rule = context.TriggerRule(CreateSavingThrow(target.Entity, context, context.MaybeCaster));
		}
		using (ContextData<SavingThrowData>.Request().Setup(rule))
		{
			Actions.Run();
		}
	}

	private RulePerformSavingThrow CreateSavingThrow(MechanicEntity unit, MechanicsContext context, MechanicEntity initiator)
	{
		return new RulePerformSavingThrow(unit, SavingThrowType, 0, initiator)
		{
			Reason = context
		};
	}

	public bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		if (Actions.Actions == null || Actions.Actions.Length == 0)
		{
			return true;
		}
		return Actions.Actions.All((GameAction a) => !(a is ContextAction contextAction) || contextAction.IsValidToCast(target, caster, casterPosition));
	}
}
