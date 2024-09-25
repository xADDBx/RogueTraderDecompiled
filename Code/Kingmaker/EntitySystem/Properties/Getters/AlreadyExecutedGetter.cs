using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("353e4860367942f8a55a59dbfb42578a")]
public class AlreadyExecutedGetter : PropertyGetter
{
	protected override int GetBaseValue()
	{
		if ((ContextData<SavableTriggerData>.Current?.ExecutesCount ?? 0) <= 0)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is trigger already executed";
	}
}
