using System.Collections.Generic;
using Kingmaker.QA.Arbiter.GameCore;

namespace Kingmaker.QA.Arbiter.Tasks;

public class ResetToMainMenuTask : ArbiterTask
{
	public ResetToMainMenuTask(ArbiterTask parentTask)
		: base(parentTask)
	{
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		if (!ArbiterIntegration.IsMainMenuActive())
		{
			base.Status = "Reset to main menu";
			Game.Instance.ResetToMainMenu();
			yield return new GameLoadingWaitTask(this);
			yield return new WaitTask(this, () => ArbiterIntegration.IsMainMenuActive());
		}
	}
}
