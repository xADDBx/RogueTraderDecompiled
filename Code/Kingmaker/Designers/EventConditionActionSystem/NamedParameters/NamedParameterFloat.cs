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
			Element.LogError(this, "Cannot find position {0} in context parameters", Parameter);
		}
		if (value != null && !(value is float))
		{
			Element.LogError(this, "WTF");
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
