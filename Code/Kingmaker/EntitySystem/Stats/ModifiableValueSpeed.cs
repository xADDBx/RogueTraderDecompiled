using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueSpeed : ModifiableValue, IHashable
{
	protected override int MinValue => 5;

	public float Mps => base.ModifiedValue.Feet().Meters / 2.5f;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
