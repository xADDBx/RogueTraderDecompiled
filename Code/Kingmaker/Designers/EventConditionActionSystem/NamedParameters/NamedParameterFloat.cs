using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("f217583c21733e24fbf40052e22f3466")]
public class NamedParameterFloat : FloatEvaluator
{
	public string Parameter;

	protected override float GetValueInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return 0f;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			PFLog.Default.Error("Cannot find position " + Parameter + " in context parameters", this);
		}
		if (value != null && !(value is float))
		{
			PFLog.Default.Warning("WTF");
		}
		if (value != null)
		{
			return (float)value;
		}
		return 0f;
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
