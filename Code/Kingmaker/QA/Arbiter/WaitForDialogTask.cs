using System.Collections.Generic;
using Kingmaker.GameModes;
using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter;

public class WaitForDialogTask : ArbiterTask
{
	public WaitForDialogTask(ArbiterTask parent)
		: base(parent)
	{
	}

	protected override IEnumerator<ArbiterTask> Routine()
	{
		yield return null;
		if (Game.Instance.CurrentMode == GameModeType.Dialog)
		{
			Game.Instance.DialogController.StopDialog();
		}
		yield return null;
	}
}
