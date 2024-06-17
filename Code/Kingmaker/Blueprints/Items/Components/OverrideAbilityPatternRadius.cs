using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("71fde9053294485a949e7871e904782f")]
public class OverrideAbilityPatternRadius : UnitFactComponentDelegate, IHashable
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public ContextValue AddToRadius;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartAbilityPatternSettings>().Add(this);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityPatternSettings>()?.Remove(this);
	}

	public AoEPattern OverrideRadius(AbilityData ability, AoEPattern originPattern)
	{
		if (!Restriction.IsPassed(base.Fact, base.Context, null, ability))
		{
			return originPattern;
		}
		int num = AddToRadius.Calculate(base.Context);
		return AoEPattern.CopyAndOverrideRadius(originPattern, Mathf.Max(0, originPattern.Radius + num));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
