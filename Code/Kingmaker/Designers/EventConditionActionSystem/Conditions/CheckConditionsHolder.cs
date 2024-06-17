using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("e7954d43ad5cff74d8fe2efd30388d73")]
public class CheckConditionsHolder : Condition
{
	[ValidateNotNull]
	public ConditionsReference ConditionsHolder;

	public ParametrizedContextSetter Parameters;

	protected override string GetConditionCaption()
	{
		return $"CheckConditionsHolder : {ConditionsHolder}";
	}

	protected override bool CheckCondition()
	{
		NamedParametersContext namedParametersContext = null;
		ParametrizedContextSetter.ParameterEntry[] array = (Parameters?.Parameters).EmptyIfNull();
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
		{
			namedParametersContext = namedParametersContext ?? new NamedParametersContext();
			namedParametersContext.Params[parameterEntry.Name] = parameterEntry.GetValue();
		}
		using (namedParametersContext?.RequestContextData())
		{
			return ConditionsHolder.Get().Conditions.Check();
		}
	}
}
