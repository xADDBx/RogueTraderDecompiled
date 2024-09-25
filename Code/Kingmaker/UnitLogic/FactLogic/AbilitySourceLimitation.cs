using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("9ed84940fa824243a3922d86ae07aadc")]
public class AbilitySourceLimitation : UnitFactComponentDelegate, IHashable
{
	public WarhammerAbilityParamsSource Sources;

	protected override void OnActivateOrPostLoad()
	{
		base.Fact.Owner.GetOrCreate<UnitPartForbiddenAbilities>().AddEntry(Sources, base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Fact.Owner.GetOptional<UnitPartForbiddenAbilities>()?.RemoveEntry(base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
