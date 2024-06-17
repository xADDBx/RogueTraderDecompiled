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
[TypeId("dee683dec5044013980e0bb396bd3c46")]
public class WarhammerDisableAttacksWithChosenWeapon : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<WarhammerUnitPartDisableAttack>()?.RetainDisabled(base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<WarhammerUnitPartDisableAttack>()?.ReleaseDisabled(base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
