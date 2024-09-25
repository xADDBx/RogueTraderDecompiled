using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("80bcc9b54f1a4ae2849b8f449d86e0ea")]
public class ContextActionMoveToCaster : ContextAction
{
	public override string GetCaption()
	{
		return "Move to caster";
	}

	protected override void RunAction()
	{
		if (!(base.Caster is BaseUnitEntity baseUnitEntity) || !(base.Target?.Entity is BaseUnitEntity baseUnitEntity2) || !(baseUnitEntity2.View != null) || !baseUnitEntity2.CanMove)
		{
			return;
		}
		WarhammerPathPlayer warhammerPathPlayer = PathfindingService.Instance.FindPathTB_Blocking(baseUnitEntity2.MovementAgent, baseUnitEntity, limitRangeByActionPoints: false);
		using (PathDisposable<WarhammerPathPlayer>.Get(warhammerPathPlayer, baseUnitEntity))
		{
			if (warhammerPathPlayer.vectorPath.Count > 0 && warhammerPathPlayer.vectorPath.Last().GetNearestNodeXZ().TryGetUnit(out var unit) && unit != null)
			{
				warhammerPathPlayer.vectorPath.RemoveAt(warhammerPathPlayer.vectorPath.Count - 1);
			}
			base.AbilityContext.TemporarilyBlockLastPathNode(warhammerPathPlayer, baseUnitEntity2);
			UnitMoveToProperParams cmdParams = new UnitMoveToProperParams(ForcedPath.Construct(warhammerPathPlayer), 0f);
			baseUnitEntity2?.Commands.Run(cmdParams);
		}
	}
}
