using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("7d0a783e64b02634a925e0ee07fc5682")]
public class NamedLocatorTransform : TransformEvaluator
{
	public string Parameter;

	protected override Transform GetValueInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return null;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			PFLog.Default.Error("Cannot find locator " + Parameter + " in context parameters", this);
		}
		if (!(value is LocatorEntity locatorEntity))
		{
			return null;
		}
		return locatorEntity.View.ViewTransform;
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
