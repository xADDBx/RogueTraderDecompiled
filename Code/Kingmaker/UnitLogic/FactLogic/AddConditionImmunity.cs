using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add condition immunity")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("c90fcf2050a28654c8d7dae6e90e204b")]
public class AddConditionImmunity : UnitFactComponentDelegate, IHashable
{
	public UnitCondition Condition;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.State.AddConditionImmunity(Condition, base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.State.RemoveConditionImmunity(Condition, base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
