using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules.Damage;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("5be3ffdc12ddc2a458ff9522c1fefdf2")]
public class ContextActionDealCrewDamage : ContextAction
{
	public int Amount;

	public bool ToCaster;

	public override string GetCaption()
	{
		return "Deal crew damage";
	}

	protected override void RunAction()
	{
		if (!((ToCaster ? base.Context.MaybeCaster : base.Target.Entity) is StarshipEntity target))
		{
			Element.LogError(this, "Target unit is missing");
		}
		else
		{
			base.Context.TriggerRule(new RuleDealCrewDamage(base.Context.MaybeCaster, target, Amount));
		}
	}
}
