using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("f21f7bc3861096d45b4081cd1c1a55c3")]
public class IntToFloat : FloatEvaluator
{
	[SerializeReference]
	public IntEvaluator Value;

	protected override float GetValueInternal()
	{
		return Value.GetValue();
	}

	public override string GetCaption()
	{
		return $"{Value}";
	}
}
