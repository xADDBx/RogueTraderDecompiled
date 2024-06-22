using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Starships;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("9fa195b8bf73baf49bd8bef276fd8fc0")]
public class WarhammerContextActionIncreaseMilitaryRating : ContextAction
{
	public int Value;

	public override string GetCaption()
	{
		return $"Increase Military Rating by {Value}";
	}

	protected override void RunAction()
	{
		if (!(base.Context.MainTarget?.Entity is StarshipEntity target))
		{
			Element.LogError(this, "Target is missing");
		}
		else
		{
			Rulebook.Trigger(new RuleStarshipPerformIncreaseMilitaryRating(base.Context.MaybeCaster, target, Value));
		}
	}
}
