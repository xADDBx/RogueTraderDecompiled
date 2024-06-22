using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5fd4ac532f6e1d148af09cb8321fe656")]
public class ContextActionDealStarshipMoraleDamage : ContextAction
{
	public int Amount;

	public bool ToCaster;

	public override string GetCaption()
	{
		return "Deal morale damage";
	}

	protected override void RunAction()
	{
		if (!((ToCaster ? base.Context.MaybeCaster : base.Target.Entity) is StarshipEntity target))
		{
			Element.LogError(this, "Target unit is missing");
		}
		else
		{
			base.Context.TriggerRule(new RuleDealStarshipMoraleDamage(base.Context.MaybeCaster, target, Amount));
		}
	}
}
