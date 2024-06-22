using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("53a93042d14d4823985c1e630eedf10b")]
public class FakeAttackType : UnitFactComponentDelegate, IHashable
{
	public bool CountAsMelee;

	public bool CountAsScatter;

	public bool CountAsAoE;

	public bool CountAsSingleShot;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
