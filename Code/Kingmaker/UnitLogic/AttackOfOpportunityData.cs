using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public struct AttackOfOpportunityData : IHashable
{
	[JsonProperty]
	public readonly BaseUnitEntity Attacker;

	[JsonProperty(IsReference = false)]
	public readonly Vector3 Position;

	[JsonProperty]
	[CanBeNull]
	public readonly BlueprintAbility Reason;

	public AttackOfOpportunityData([NotNull] BaseUnitEntity attacker, Vector3 position)
	{
		Attacker = attacker ?? throw new ArgumentNullException("attacker");
		Position = position;
		Reason = null;
	}

	public AttackOfOpportunityData([NotNull] BaseUnitEntity attacker, Vector3 position, [CanBeNull] BlueprintAbility reason)
	{
		Attacker = attacker ?? throw new ArgumentNullException("attacker");
		Position = position;
		Reason = reason;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<BaseUnitEntity>.GetHash128(Attacker);
		result.Append(ref val);
		Vector3 val2 = Position;
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Reason);
		result.Append(ref val3);
		return result;
	}
}
