using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("49799ef400df6a4459812087dbf8b1d4")]
public class FloatConstant : FloatEvaluator
{
	public float Value;

	public override string GetDescription()
	{
		return $"Возвращает вещественное число {Value}";
	}

	protected override float GetValueInternal()
	{
		return Value;
	}

	public override string GetCaption()
	{
		return $"{Value}";
	}
}
