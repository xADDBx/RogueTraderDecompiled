using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("88c33e2e171c44948b372a22e3250c43")]
public class NamedParameterBlueprint : BlueprintEvaluator
{
	public string Parameter;

	protected override BlueprintScriptableObject GetValueInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return null;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			Element.LogError(this, "Cannot find blueprint {0} in context parameters", Parameter);
		}
		if (value is string)
		{
			return ResourcesLibrary.TryGetBlueprint((string)value) as BlueprintScriptableObject;
		}
		return value as BlueprintScriptableObject;
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
