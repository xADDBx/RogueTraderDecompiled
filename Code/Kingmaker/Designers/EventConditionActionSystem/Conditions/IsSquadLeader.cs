using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("8e743a8a4a9844f6922dc6c0da8a9bd3")]
public class IsSquadLeader : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override bool CheckCondition()
	{
		return Unit.GetValue().IsSquadLeader;
	}

	protected override string GetConditionCaption()
	{
		return $"{Unit} is squad leader";
	}
}
