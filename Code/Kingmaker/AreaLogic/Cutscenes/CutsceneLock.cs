using Kingmaker.GameCommands;
using Kingmaker.GameModes;

namespace Kingmaker.AreaLogic.Cutscenes;

public static class CutsceneLock
{
	private static bool ShouldBeActive
	{
		get
		{
			foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
			{
				if (!cutscene.IsFinished && !cutscene.Paused && cutscene.Cutscene.LockControl)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool Active
	{
		get
		{
			if (!(Game.Instance.CurrentMode == GameModeType.Cutscene))
			{
				return Game.Instance.CurrentMode == GameModeType.CutsceneGlobalMap;
			}
			return true;
		}
	}

	public static void CheckRequest()
	{
		if (!Active && ShouldBeActive)
		{
			Game.Instance.GameCommandQueue.ScheduleSwitchCutsceneLock(@lock: true);
		}
	}

	public static void CheckRelease()
	{
		if (Active && !ShouldBeActive)
		{
			Game.Instance.GameCommandQueue.ScheduleSwitchCutsceneLock(@lock: false);
		}
	}
}
