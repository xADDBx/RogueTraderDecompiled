using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.Utils;

public static class CutsceneUIState
{
	private sealed class Handler : IGameModeHandler, ISubscriber
	{
		public void OnGameModeStart(GameModeType gameMode)
		{
			if (gameMode == GameModeType.Cutscene)
			{
				IsCutsceneActive.Value = true;
			}
		}

		public void OnGameModeStop(GameModeType gameMode)
		{
			if (gameMode == GameModeType.Cutscene)
			{
				IsCutsceneActive.Value = false;
			}
		}
	}

	public static readonly BoolReactiveProperty IsCutsceneActive = new BoolReactiveProperty(initialValue: false);

	private static readonly Handler _Handler = new Handler();

	public static bool IsForegroundCutsceneActive
	{
		get
		{
			foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
			{
				if (!cutscene.IsFinished && !cutscene.Paused && (cutscene.Cutscene.LockControl || cutscene.Cutscene.ShowOverlay))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static IDisposable Initialize()
	{
		return EventBus.Subscribe(_Handler);
	}
}
