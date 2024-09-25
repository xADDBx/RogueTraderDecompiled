using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4d7f63e0f8854d8496308bdc0a5e6083")]
public class WarhammerConcentrationActionPoints : UnitFactComponentDelegate, IHashable
{
	public int ConcentrationActionPointsBonus;

	protected override void OnActivate()
	{
		base.Owner.GetOrCreate<WarhammerUnitPartConcentrationController>().AdditionalConcentrationActionPoints += ConcentrationActionPointsBonus;
	}

	protected override void OnDeactivate()
	{
		WarhammerUnitPartConcentrationController optional = base.Owner.GetOptional<WarhammerUnitPartConcentrationController>();
		if (optional != null)
		{
			optional.AdditionalConcentrationActionPoints -= ConcentrationActionPointsBonus;
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
