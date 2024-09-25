using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("e63470ddc5d847eb9c563e5240537eec")]
public class BladeDanceFeature : UnitFactComponentDelegate, IHashable
{
	public BlueprintAbilityReference BladeDanceAbility;

	public ContextValue RateOfAttack;

	protected override void OnRecalculate()
	{
		AbilityData abilityData = base.Owner.Abilities.Enumerable.FirstOrDefault((Ability p) => p.Blueprint == BladeDanceAbility.GetBlueprint())?.Data;
		if (abilityData != null)
		{
			abilityData.OverrideRateOfFire = RateOfAttack.Calculate(base.Context);
		}
	}

	protected override void OnPostLoad()
	{
		if (base.Fact.Active)
		{
			OnRecalculate();
		}
	}

	protected override void OnActivate()
	{
		OnRecalculate();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
