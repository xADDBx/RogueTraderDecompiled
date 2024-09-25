using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("dbc6e3efabe1d4e4f819632c80ca360e")]
public class WarhammerContextActionDecreaseMilitaryRating : ContextAction
{
	public int Value;

	public bool ToCaster;

	public override string GetCaption()
	{
		return string.Format("Decrease {0} Military Rating by {1}", ToCaster ? "caster" : "target", Value);
	}

	protected override void RunAction()
	{
		if (!((ToCaster ? base.Context.MaybeCaster : base.Context.MainTarget?.Entity) is StarshipEntity target))
		{
			Element.LogError(this, "Target is missing");
		}
		else
		{
			Rulebook.Trigger(new RuleStarshipPerformDecreaseMilitaryRating(base.Context.MaybeCaster, target, Value));
		}
	}
}
