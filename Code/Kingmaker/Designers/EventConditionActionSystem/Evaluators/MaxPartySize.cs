using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/MaxPartySize")]
[AllowMultipleComponents]
[TypeId("e3e6a8aefd66ec5469f8b81f3b370df5")]
public class MaxPartySize : IntEvaluator
{
	protected override int GetValueInternal()
	{
		return 6;
	}

	public override string GetCaption()
	{
		return "Max Party Size";
	}
}
