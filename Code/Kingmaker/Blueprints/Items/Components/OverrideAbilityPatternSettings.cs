using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.PatternAttack;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("2968fb1da46b4f719f835b33145974f1")]
public class OverrideAbilityPatternSettings : UnitFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public AbilityAoEPatternSettings PatternSettings = new AbilityAoEPatternSettings();

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityPatternSettings>().Add(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityPatternSettings>()?.Remove(this);
	}

	[CanBeNull]
	public AbilityAoEPatternSettings GetPatternSettings(AbilityData ability)
	{
		if (!Restriction.IsPassed(base.Fact, base.Context, null, ability))
		{
			return null;
		}
		return PatternSettings;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
