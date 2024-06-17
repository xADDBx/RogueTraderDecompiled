using System.Collections;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;

namespace Kingmaker.QA.Clockwork;

internal class TaskAutoClickSystemDialog : ClockworkRunnerTask
{
	public TaskAutoClickSystemDialog(ClockworkRunner runner)
		: base(runner)
	{
	}

	protected override IEnumerator Routine()
	{
		DialogController controller = Game.Instance.DialogController;
		while (controller.Answers.Count() == 1 && controller.Answers.First().IsSystem())
		{
			controller.SelectAnswer(controller.Answers.First());
			yield return null;
			yield return 1f;
		}
	}

	public override string ToString()
	{
		return "Auto click dialog button (Continue/Exit/etc)";
	}
}
