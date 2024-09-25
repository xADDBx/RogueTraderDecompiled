using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using StateHasher.Core;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("6e5f0ad24f67ebf45bf5017551dce11b")]
public class StarshipMovementVariation : EntityFactComponentDelegate<StarshipEntity>, IUnitCommandActHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private ShipPath.TurnAngleType nextTurnAngle;

	[SerializeField]
	private bool disableAfterTurnDone;

	public ShipPath.TurnAngleType NextTurnAngle => nextTurnAngle;

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (!disableAfterTurnDone || command.Executor != base.Owner || !(command is UnitMoveToProper unitMoveToProper))
		{
			return;
		}
		Vector3 forward = base.Owner.Forward;
		List<Vector3> vectorPath = unitMoveToProper.ForcedPath.vectorPath;
		for (int i = 1; i < vectorPath.Count; i++)
		{
			Vector3 normalized = (vectorPath[i] - vectorPath[i - 1]).normalized;
			if ((double)Vector3.Dot(forward, normalized) < 0.95)
			{
				base.Owner.Facts.Remove(base.Fact);
				break;
			}
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Navigation.OverrideTurnAngles(new ShipPath.TurnAngleType[1] { nextTurnAngle });
	}

	protected override void OnDeactivate()
	{
		base.Owner.Navigation.ResetCustomOverrides();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
