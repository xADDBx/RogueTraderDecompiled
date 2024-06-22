using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("bf4546d1adee47989b00198824c424ee")]
public class NamedParameterMapObjectTransform : TransformEvaluator
{
	public string Parameter;

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}

	protected override Transform GetValueInternal()
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
		if (!(value is MapObjectEntity mapObjectEntity))
		{
			return null;
		}
		return mapObjectEntity.View.ViewTransform;
	}
}
