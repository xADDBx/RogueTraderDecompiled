using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("97f0ad03ca55ccb4692cfc80b7e626ea")]
public class NamedParameterPosition : PositionEvaluator
{
	public string Parameter;

	protected override Vector3 GetValueInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return Vector3.zero;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			PFLog.Default.Error("Cannot find position " + Parameter + " in context parameters", this);
		}
		if (value != null && !(value is Vector3))
		{
			PFLog.Default.Warning("WTF");
		}
		if (value != null)
		{
			return (Vector3)value;
		}
		return Vector3.zero;
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
