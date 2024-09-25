using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("12c0b652aac8a2a4abf467858236cd75")]
public class MovedInCombatTrigger : UnitFactComponentDelegate, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public ActionList Actions;

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper unitMoveToProper && base.Context.MaybeOwner == command.Executor)
		{
			base.Context[ContextPropertyName.Value1] = (int)unitMoveToProper.MovePointsSpent;
			base.Fact.RunActionInContext(Actions, base.Context.MaybeOwner.ToITargetWrapper());
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
