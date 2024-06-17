using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("fafdce45c270d9547b4d6f26518983dc")]
public class ContextActionHealStarshipMoraleDamage : ContextAction
{
	public int Amount;

	public bool IsRecoverFullMorale;

	public override string GetCaption()
	{
		return "Heal morale damage";
	}

	public override void RunAction()
	{
		if (!(base.Target.Entity is StarshipEntity target))
		{
			PFLog.Default.Error("Target unit is missing");
		}
		else
		{
			base.Context.TriggerRule(new RuleHealStarshipMoraleDamage(base.Context.MaybeCaster, target, Amount, IsRecoverFullMorale));
		}
	}
}
