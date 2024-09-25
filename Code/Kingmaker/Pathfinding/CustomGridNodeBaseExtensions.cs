using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Pathfinding;

public static class CustomGridNodeBaseExtensions
{
	public static bool ContainsUnit(this CustomGridNodeBase node)
	{
		return Game.Instance.CustomGridNodeController.ContainsUnit(node);
	}

	public static bool ContainsUnit(this CustomGridNodeBase node, BaseUnitEntity unit)
	{
		return Game.Instance.CustomGridNodeController.ContainsUnit(node, unit);
	}

	public static bool TryGetUnit(this CustomGridNodeBase node, out BaseUnitEntity unit)
	{
		return Game.Instance.CustomGridNodeController.TryGetUnit(node, out unit);
	}

	[CanBeNull]
	public static BaseUnitEntity GetUnit(this CustomGridNodeBase node)
	{
		return Game.Instance?.CustomGridNodeController.GetUnit(node);
	}
}
