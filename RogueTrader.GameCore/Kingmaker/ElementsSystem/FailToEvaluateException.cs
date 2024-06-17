using JetBrains.Annotations;

namespace Kingmaker.ElementsSystem;

public class FailToEvaluateException : ElementLogicException
{
	public Evaluator Evaluator => (Evaluator)Element;

	public FailToEvaluateException([NotNull] Evaluator evaluator)
		: base(evaluator)
	{
	}

	public override string GetPrefixText()
	{
		return "Fail to evaluate";
	}
}
