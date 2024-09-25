using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[ComponentName("Buffs/Generic stat bonus")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("a0c7e67ead2fb1b469484150184b3d4a")]
public class AddGenericStatBonus : UnitBuffComponentDelegate, IHashable
{
	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public int Value = 1;

	protected override void OnActivateOrPostLoad()
	{
		int value = AddStatBonus.TryApplyArcanistPowerfulChange(base.Context, Stat, Value);
		base.Owner.Stats.GetStat(Stat)?.AddModifier(value, base.Runtime, Descriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat)?.RemoveModifiersFrom(base.Runtime);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
