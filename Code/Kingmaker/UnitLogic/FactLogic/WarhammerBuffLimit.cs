using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[AllowMultipleComponents]
[TypeId("e4ab18cafc96da542b41e64983474221")]
public class WarhammerBuffLimit : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<WarhammerUnitPartBuffLimit>().AddWatchedBuff(Buff);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
