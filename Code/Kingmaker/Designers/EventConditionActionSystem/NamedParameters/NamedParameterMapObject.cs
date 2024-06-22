using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("f1f3f66a7a5690a42ab161fdd0b1de19")]
public class NamedParameterMapObject : MapObjectEvaluator
{
	public string Parameter;

	protected override MapObjectEntity GetMapObjectInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return null;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			Element.LogError(this, "Cannot find mapobj {0} in context parameters", Parameter);
		}
		return value as MapObjectEntity;
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
