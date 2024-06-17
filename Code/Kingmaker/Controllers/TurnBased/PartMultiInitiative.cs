using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class PartMultiInitiative : MechanicEntityPart, IHashable
{
	public int AdditionalTurnsCount { get; private set; }

	public void Setup(int additionalTurns)
	{
		AdditionalTurnsCount = additionalTurns;
	}

	public IEnumerable<InitiativePlaceholderEntity> EnsurePlaceholders()
	{
		return from i in Enumerable.Range(0, AdditionalTurnsCount)
			select InitiativePlaceholderEntity.Ensure(base.Owner, i);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
