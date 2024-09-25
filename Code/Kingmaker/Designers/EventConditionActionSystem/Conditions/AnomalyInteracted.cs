using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Exploration;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("1e8f415e58564ae4fae2afd4a9cdf522")]
public class AnomalyInteracted : Condition
{
	[SerializeReference]
	public StarSystemObjectOnScene ObjectEvaluator;

	protected override string GetConditionCaption()
	{
		return "Was anomaly on this object interacted";
	}

	protected override bool CheckCondition()
	{
		if (!ObjectEvaluator.TryGetValue(out var value) || !(value is AnomalyEntityData))
		{
			return false;
		}
		return ((AnomalyEntityData)value).IsInteracted;
	}
}
