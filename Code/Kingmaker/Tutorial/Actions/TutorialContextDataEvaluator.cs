using System;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Tutorial.Actions;

[Serializable]
public class TutorialContextDataEvaluator
{
	public TutorialContextKey Key;

	[ValidateNotNull]
	[SerializeReference]
	public Evaluator Evaluator;

	public object GetValue()
	{
		if (Key.IsUnit())
		{
			return ((AbstractUnitEvaluator)Evaluator)?.GetValue();
		}
		if (Key.IsItem())
		{
			return ((ItemFromUnitEvaluator)Evaluator)?.GetValue();
		}
		if (Key.IsAbility())
		{
			return ((AbilityEvaluator)Evaluator)?.GetValue();
		}
		throw new Exception($"Unsupported Key {Key}");
	}
}
