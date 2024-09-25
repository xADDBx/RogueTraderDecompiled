using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d000fc1491094c8e93f912c02cb86ecc")]
public class WarhammerFreeUltimateBuff : UnitBuffComponentDelegate, IHashable
{
	[SerializeField]
	private bool m_NoMomentumCost;

	public bool NoMomentumCost => m_NoMomentumCost;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
