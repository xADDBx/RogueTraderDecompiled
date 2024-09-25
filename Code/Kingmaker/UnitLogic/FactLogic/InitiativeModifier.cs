using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("ec6f18eabc484e73bc1858674c651318")]
public class InitiativeModifier : UnitFactComponentDelegate, IHashable
{
	public float OverrideInitiative;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
