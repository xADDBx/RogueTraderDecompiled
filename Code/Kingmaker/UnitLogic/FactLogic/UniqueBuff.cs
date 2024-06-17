using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("0f4abcc6f460e094eb0754103a4bdef7")]
public class UniqueBuff : UnitBuffComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		base.Buff.Context.MaybeCaster?.GetOrCreate<UnitPartUniqueBuffs>().NewBuff(base.Buff);
	}

	protected override void OnDeactivate()
	{
		base.Buff.Context.MaybeCaster?.GetOptional<UnitPartUniqueBuffs>()?.RemoveBuff(base.Buff);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
