using System.Collections;

namespace Kingmaker.QA.Arbiter;

public class ResetToMainMenuTask : ArbiterTask
{
	public ResetToMainMenuTask(ArbiterTask parentTask)
		: base(parentTask)
	{
	}

	protected override IEnumerator Routine()
	{
		if (!ArbiterClientIntegration.IsMainMenuActive())
		{
			base.Status = "Reset to main menu";
			Game.Instance.ResetToMainMenu();
			yield return new GameLoadingWaitTask(this);
			yield return new WaitTask(this, () => ArbiterClientIntegration.IsMainMenuActive());
		}
	}
}
