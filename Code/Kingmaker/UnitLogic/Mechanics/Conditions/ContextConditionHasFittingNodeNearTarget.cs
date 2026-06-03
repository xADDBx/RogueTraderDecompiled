using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("7dacedaab3f02864eabf1c08339daa1e")]
public class ContextConditionHasFittingNodeNearTarget : ContextCondition
{
	private static int PLACEMENT_RADIUS = 1;

	protected override string GetConditionCaption()
	{
		return "Check if there is enough space near the target to accommodate";
	}

	protected override bool CheckCondition()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		MechanicEntity entity = base.Target.Entity;
		NodeList targetNodes = entity.GetOccupiedNodes();
		int radius = Math.Max(maybeCaster.SizeRect.Height, maybeCaster.SizeRect.Width);
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(entity.CurrentUnwalkableNode, entity.SizeRect, radius))
		{
			if (maybeCaster.CanStandHere(item))
			{
				NodeList nodes = GridAreaHelper.GetNodes(item, maybeCaster.SizeRect);
				if (!nodes.Any((CustomGridNodeBase c) => targetNodes.Any((CustomGridNodeBase t) => c.CellDistanceTo(t) == 0)) && nodes.Any((CustomGridNodeBase c) => targetNodes.Any((CustomGridNodeBase t) => c.CellDistanceTo(t) == 1)))
				{
					return true;
				}
			}
		}
		return false;
	}
}
