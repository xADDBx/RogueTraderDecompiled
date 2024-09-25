using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.QA.Arbiter;

public class GameLoadingWaitTask : WaitTask
{
	public GameLoadingWaitTask(ArbiterTask parent)
		: base(parent)
	{
	}

	protected override bool Predicate()
	{
		if ((bool)LoadingProcess.Instance.IsAwaitingUserInput)
		{
			LoadingProcess.Instance.IsAwaitingUserInput.Release();
		}
		return !LoadingProcess.Instance.IsLoadingInProcess;
	}
}
