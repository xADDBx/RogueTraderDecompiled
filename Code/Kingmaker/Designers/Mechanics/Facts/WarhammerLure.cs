using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("6516ca729174cd34fa17868e6b846069")]
public class WarhammerLure : UnitBuffComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<UnitPartLure>().UnitLuredTo = base.Context?.MaybeCaster as BaseUnitEntity;
	}

	protected override void OnDeactivate()
	{
		UnitPartLure optional = base.Owner.GetOptional<UnitPartLure>();
		if (optional != null)
		{
			optional.UnitLuredTo = null;
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
