using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[TypeId("17b7e5e565e0db44d972ec8999f57106")]
public class AbilityGroupLimitation : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintAbilityGroupReference m_Group;

	public BlueprintAbilityGroup Group => m_Group?.Get();

	protected override void OnActivateOrPostLoad()
	{
		base.Fact.Owner.GetOrCreate<UnitPartForbiddenAbilities>().AddEntry(Group, base.Fact);
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
