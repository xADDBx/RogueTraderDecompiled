using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("1bd8f9b17dea44eebf0ed90c8dc7cdcc")]
public class ModifierPsychicPhenomenaChance : UnitFactComponentDelegate, IHashable
{
	public ContextValue AdditionChanceOnPsychicPhenomena;

	public ContextValue AdditionChanceOnPerilsOfWarp;

	protected override void OnActivateOrPostLoad()
	{
		PartPsyker optional = base.Owner.Parts.GetOptional<PartPsyker>();
		if (optional != null)
		{
			optional.AdditionChanceOnPerilsOfWarp = AdditionChanceOnPerilsOfWarp.Calculate(base.Context);
			optional.AdditionChanceOnPsychicPhenomena = AdditionChanceOnPsychicPhenomena.Calculate(base.Context);
		}
	}

	protected override void OnDeactivate()
	{
		PartPsyker optional = base.Owner.Parts.GetOptional<PartPsyker>();
		if (optional != null)
		{
			optional.AdditionChanceOnPerilsOfWarp = 0;
			optional.AdditionChanceOnPsychicPhenomena = 0;
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
