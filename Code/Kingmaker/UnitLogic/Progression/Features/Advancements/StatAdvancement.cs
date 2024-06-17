using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features.Advancements;

[Serializable]
[AllowedOn(typeof(BlueprintStatAdvancement))]
[TypeId("1193f055d9ae4f5aa45e27400d7411cd")]
public class StatAdvancement : UnitFactComponentDelegate, IHashable
{
	private BlueprintStatAdvancement Settings => (BlueprintStatAdvancement)base.Fact.Blueprint;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Stats.GetStat(Settings.Stat).AddModifier(Settings.ValuePerRank * base.Fact.GetRank(), base.Runtime, Settings.ModifierDescriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Settings.Stat).RemoveModifiersFrom(base.Runtime);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
