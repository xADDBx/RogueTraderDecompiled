using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/IntConstant")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("9054151c7eb2e854ca7109522b1c3dde")]
public class IntConstant : IntEvaluator
{
	public int Value;

	protected override int GetValueInternal()
	{
		return Value;
	}

	public override string GetCaption()
	{
		return Value.ToString();
	}
}
