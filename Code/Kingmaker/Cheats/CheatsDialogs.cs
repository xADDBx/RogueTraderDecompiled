using Core.Cheats;

namespace Kingmaker.Cheats;

internal static class CheatsDialogs
{
	[Cheat(Name = "stop_dialog", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StopDialog()
	{
		if (Game.Instance.DialogController.Dialog != null)
		{
			Game.Instance.DialogController.StopDialog(force: true);
		}
	}
}
