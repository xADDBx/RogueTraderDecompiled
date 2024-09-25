using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[TypeId("95469ff256aabcf409b9c5860a4c4ba9")]
public class NamedParameterUnit : AbstractUnitEvaluator
{
	public string Parameter;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		NamedParametersContext.ContextData current = ContextData<NamedParametersContext.ContextData>.Current;
		if (current == null)
		{
			return null;
		}
		if (!current.Context.Params.TryGetValue(Parameter, out var value))
		{
			Element.LogError(this, "Cannot find unit {0} in context parameters", Parameter);
		}
		if (value is string uniqueId)
		{
			UnitReference unitReference = new UnitReference(uniqueId);
			current.Context.Params[Parameter] = unitReference;
			return unitReference.ToAbstractUnitEntity();
		}
		return (value as IEntityRef)?.Get<AbstractUnitEntity>();
	}

	public override string GetCaption()
	{
		return "P:" + Parameter;
	}
}
