namespace Kingmaker.QA.Arbiter;

public interface IArbiterCheckerComponent
{
	ArbiterTask GetArbiterTask(ArbiterStartupParameters arguments);
}
