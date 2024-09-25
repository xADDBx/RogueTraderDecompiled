using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("de7e7ea46548a14438b6ce6e738ca309")]
public class WarhammerContextActionCoverActions : ContextAction
{
	public ActionList NoCoverActions;

	public ActionList CoverActions;

	public override string GetCaption()
	{
		return "Do actions depending on cover";
	}

	protected override void RunAction()
	{
		Vector3 origin;
		IntRect originSize;
		if (base.AbilityContext.Ability.GetPatternSettings() != null)
		{
			Vector3 position = base.Context.MaybeCaster.Position;
			Vector3 point = base.AbilityContext.ClickedTarget.Point;
			origin = AoEPatternHelper.GetActualCastPosition(base.AbilityContext.Caster, position, point, base.AbilityContext.Ability.MinRangeCells, base.AbilityContext.Ability.RangeCells);
			originSize = default(IntRect);
		}
		else
		{
			origin = base.Context.MaybeCaster.Position;
			originSize = base.Context.MaybeCaster.SizeRect;
		}
		IntRect sizeRect = base.Target.SizeRect;
		if ((LosCalculations.CoverType)LosCalculations.GetWarhammerLos(origin, originSize, base.Target.Point, sizeRect) == LosCalculations.CoverType.None)
		{
			NoCoverActions.Run();
		}
		else
		{
			CoverActions.Run();
		}
	}
}
