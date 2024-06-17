using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a0f6aa422d1512e46833e2365baffae4")]
public class SuicideAttacker : UnitFactComponentDelegate, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCommandStartHandler, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public BaseUnitEntity UnitOnFinishPosition;
	}

	public ActionList ActionsOnTarget;

	public ActionList ActionOnSelf;

	private CustomGridGraph Graph => base.Owner.GetStarshipNavigationOptional()?.ReachableTiles.startNode.Graph as CustomGridGraph;

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor == base.Owner && command is UnitMoveToProper unitMoveToProper)
		{
			List<Vector3> vectorPath = unitMoveToProper.ForcedPath.vectorPath;
			BaseUnitEntity unit = vectorPath[vectorPath.Count - 1].ToNode(Graph).GetUnit();
			RequestTransientData<ComponentData>().UnitOnFinishPosition = unit;
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor == base.Owner && command is UnitMoveToProper)
		{
			ComponentData componentData = RequestTransientData<ComponentData>();
			if (componentData.UnitOnFinishPosition != null)
			{
				base.Fact.RunActionInContext(ActionsOnTarget, componentData.UnitOnFinishPosition.ToITargetWrapper());
				base.Fact.RunActionInContext(ActionOnSelf, base.OwnerTargetWrapper);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
