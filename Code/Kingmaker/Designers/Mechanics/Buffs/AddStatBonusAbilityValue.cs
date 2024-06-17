using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[ComponentName("Add stat bonus from ability value")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("c6f48aa9766288d408eeac7a9f767c74")]
public class AddStatBonusAbilityValue : UnitBuffComponentDelegate, IHashable
{
	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public ContextValue Value;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Stats.GetStat(Stat)?.AddModifier(Value.Calculate(base.Context), base.Runtime, Descriptor);
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
