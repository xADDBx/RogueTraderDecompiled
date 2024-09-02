using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("002a7f83be294e62bce99246de4f5fb4")]
public class SpringAttack : UnitFactComponentDelegate, IHashable
{
	public BlueprintAbilityReference DeathWaltz;

	public BlueprintAbilityReference DeathWaltzUltimate;

	public BlueprintAbilityAreaEffectReference AreaMark;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartSpringAttack>().DeathWaltzBlueprint = DeathWaltz;
		base.Owner.GetOrCreate<UnitPartSpringAttack>().DeathWaltzUltimateBlueprint = DeathWaltzUltimate;
		base.Owner.GetOrCreate<UnitPartSpringAttack>().AreaMark = AreaMark;
		base.Owner.GetOrCreate<UnitPartSpringAttack>().SpringAttackFeature = base.Fact;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
