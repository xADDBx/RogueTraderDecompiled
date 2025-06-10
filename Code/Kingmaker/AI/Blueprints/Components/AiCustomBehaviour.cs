using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Blueprints.Components;

[TypeId("bcd9c69ea0c74b23bdf3ecbcafbadc27")]
public class AiCustomBehaviour : EntityFactComponentDelegate, IHashable
{
	public CustomBehaviourType BehaviourType;

	protected override void OnActivate()
	{
		base.Owner.GetOptional<PartUnitBrain>().SetCustomBehaviour(BehaviourType);
	}

	protected override void OnApplyPostLoadFixes()
	{
		base.Owner.GetOptional<PartUnitBrain>().SetCustomBehaviour(BehaviourType);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartUnitBrain>().SetCustomBehaviour(CustomBehaviourType.None);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
