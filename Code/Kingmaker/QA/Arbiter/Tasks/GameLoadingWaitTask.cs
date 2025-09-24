using Kingmaker.EntitySystem.Persistence;

namespace Kingmaker.QA.Arbiter.Tasks;

public class GameLoadingWaitTask : WaitTask
{
	public GameLoadingWaitTask(ArbiterTask parent)
		: base(parent)
	{
		base.Status = "Game loading wait...";
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
