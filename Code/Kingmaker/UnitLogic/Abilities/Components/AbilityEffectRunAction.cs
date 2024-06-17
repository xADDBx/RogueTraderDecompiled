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
			rule = context.TriggerRule(CreateSavingThrow(target.Entity, context, persistentSpell: false));
		}
		using (ContextData<SavingThrowData>.Request().Setup(rule))
		{
			Actions.Run();
		}
	}

	private RulePerformSavingThrow CreateSavingThrow(MechanicEntity unit, MechanicsContext context, bool persistentSpell)
	{
		return new RulePerformSavingThrow(unit, SavingThrowType, 0)
		{
			Reason = context,
			Buff = ContextActionSavingThrow.FindApplyBuffAction(Actions)?.Buff,
			PersistentSpell = persistentSpell
		};
	}
}
