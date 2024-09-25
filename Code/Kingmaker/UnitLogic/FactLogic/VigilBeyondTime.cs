using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0ec14c9118234c84bf37e3fc1b56ece6")]
public class VigilBeyondTime : UnitBuffComponentDelegate, IHashable
{
	public BlueprintAbilityReference TeleportAbility;

	protected override void OnActivate()
	{
		base.Context.MaybeCaster?.GetOrCreate<UnitPartVigilBeyondTime>().AddEntry(base.Buff, TeleportAbility);
	}

	protected override void OnDeactivate()
	{
		base.Context.MaybeCaster?.GetOrCreate<UnitPartVigilBeyondTime>().RemoveEntry(base.Buff);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
