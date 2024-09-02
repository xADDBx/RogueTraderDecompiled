using System;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

[Obsolete]
public class UnitPartSpellResistance : BaseUnitPart, IHashable
{
	public class SpellResistanceValue
	{
		[JsonProperty]
		public readonly int Id;

		[JsonProperty]
		public readonly string FactId;

		[JsonConstructor]
		public SpellResistanceValue(int id, string factId)
		{
			FactId = factId;
			Id = id;
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
