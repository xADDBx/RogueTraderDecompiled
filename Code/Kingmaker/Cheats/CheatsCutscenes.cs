using System.Linq;
using Core.Cheats;
using Kingmaker.AreaLogic.Cutscenes;

namespace Kingmaker.Cheats;

internal static class CheatsCutscenes
{
	[Cheat(Name = "stop_cutscene", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StopCutscenes(Cutscene cutscene)
	{
		foreach (CutscenePlayerData item in Game.Instance.State.Cutscenes.Where((CutscenePlayerData p) => p.Cutscene == cutscene))
		{
			item.Stop();
		}
	}
}
