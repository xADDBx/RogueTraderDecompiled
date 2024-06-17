using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Colonization;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("fa8569250c95f9b4aa7d04c5cce9319f")]
public class ColonyContentmentHigher : Condition
{
	public int Value;

	protected override string GetConditionCaption()
	{
		return "Check if current colony contentment is higher than value";
	}

	protected override bool CheckCondition()
	{
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		if (colony == null)
		{
			return false;
		}
		return colony.Contentment.Value > Value;
	}
}
