using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Mechanics.Actions;

[TypeId("a81250c2e5754eb4ba2a6ebbde4b2e65")]
public class ContextActionMedicae : ContextAction
{
	public ContextValue BaseHeal;

	public override string GetCaption()
	{
		return "Perform medicae";
	}

	public override void RunAction()
	{
		Rulebook.Trigger(new RulePerformMedicaeHeal(base.Caster, base.TargetEntity, BaseHeal.Calculate(base.Context)));
	}
}
