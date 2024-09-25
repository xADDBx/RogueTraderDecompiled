using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

[TypeId("d7288a3f190a18c429029d4f0d60c99d")]
public class ContextActionSetApToZero : ContextAction
{
	public override string GetCaption()
	{
		return "Set all action points to zero";
	}

	protected override void RunAction()
	{
		(base.Target.Entity?.GetCombatStateOptional())?.SpendActionPointsAll(yellow: true, blue: true);
	}
}
