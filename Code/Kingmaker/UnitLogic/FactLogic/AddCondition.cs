using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add condition")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("4c36aacebf153574eb39757fc3965edb")]
public class AddCondition : UnitFactComponentDelegate, IHashable
{
	public UnitCondition Condition;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.State.AddCondition(Condition, base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.State.RemoveCondition(Condition, base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
