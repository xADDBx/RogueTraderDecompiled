using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics;

public abstract class StarshipPart : BaseUnitPart<StarshipEntity>, IHashable
{
	public override Type RequiredEntityType => EntityInterfacesHelper.StarshipEntityEntityInterface;

	protected BlueprintStarship Blueprint => base.Owner.Blueprint;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
