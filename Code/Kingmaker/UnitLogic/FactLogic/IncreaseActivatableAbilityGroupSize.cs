using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("886514ed5ee24e0da4cda44316d3904b")]
public class IncreaseActivatableAbilityGroupSize : UnitFactComponentDelegate, IHashable
{
	public ActivatableAbilityGroup Group;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartActivatableAbility>().IncreaseGroupSize(Group);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<UnitPartActivatableAbility>().DecreaseGroupSize(Group);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
