using Core.Cheats;
using Kingmaker.Controllers;
using Kingmaker.GameCommands;

namespace Kingmaker.AreaLogic.Cutscenes;

public static class CutsceneCheats
{
	[Cheat(Name = "skip_cutscene")]
	public static void SkipCutscene()
	{
		Game.Instance.GameCommandQueue.SkipCutscene();
	}

	[Cheat(Name = "skip_bark")]
	public static void SkipBark()
	{
		CutsceneController.SkipBarkBanter();
	}
}
