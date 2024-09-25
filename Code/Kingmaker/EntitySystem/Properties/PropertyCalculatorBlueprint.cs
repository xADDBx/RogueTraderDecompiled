using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.EntitySystem.Properties;

[Serializable]
[TypeId("19be5dcf539a404487f98692dc73d872")]
public class PropertyCalculatorBlueprint : BlueprintScriptableObject
{
	public int Add;

	public PropertyCalculator Value;

	public int GetValue(PropertyContext context)
	{
		return Value.GetValue(context) + Add;
	}
}
