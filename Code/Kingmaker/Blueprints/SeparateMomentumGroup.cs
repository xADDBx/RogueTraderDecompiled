using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b07eecc630ff4aaeb66e603b506c5cd3")]
public class SeparateMomentumGroup : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintMomentumGroupReference m_MomentumGroup;

	public BlueprintMomentumGroup MomentumGroup => m_MomentumGroup;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
