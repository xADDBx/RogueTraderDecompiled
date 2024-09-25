using JetBrains.Annotations;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Tutorial;

public class TutorialVM : VMBase, IGameModeHandler, ISubscriber, INewTutorialUIHandler
{
	public readonly ReactiveProperty<TutorialModalWindowVM> BigWindowVM = new ReactiveProperty<TutorialModalWindowVM>();

	public readonly ReactiveProperty<TutorialHintWindowVM> SmallWindowVM = new ReactiveProperty<TutorialHintWindowVM>();

	private TutorialData m_DataToRestore;

	private bool IsShowingBigWindow => BigWindowVM.Value?.Data != null;

	private bool IsShowingSmallWindow
	{
		get
		{
			if (!IsShowingBigWindow)
			{
				return SmallWindowVM.Value?.Data != null;
			}
			return false;
		}
	}

	public TutorialVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		DisposeAndRemove(BigWindowVM);
		DisposeAndRemove(SmallWindowVM);
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if ((IsShowingBigWindow && !CanShowBigWindow(gameMode)) || (IsShowingSmallWindow && !CanShowSmallWindow(gameMode)))
		{
			m_DataToRestore = BigWindowVM.Value?.Data ?? SmallWindowVM.Value?.Data;
			BigWindowVM.Value?.Hide();
			SmallWindowVM.Value?.Hide();
		}
		else if (m_DataToRestore != null && Game.Instance.Player.Tutorial.ShowingData == null && (!IsBigWindowTutorial(m_DataToRestore) || CanShowBigWindow(gameMode)) && (IsBigWindowTutorial(m_DataToRestore) || CanShowSmallWindow(gameMode)))
		{
			ShowTutorialInternal(m_DataToRestore);
			m_DataToRestore = null;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private static bool CanShowBigWindow(GameModeType gameMode)
	{
		if (!Game.Instance.IsModeActive(GameModeType.Cutscene) && !Game.Instance.IsModeActive(GameModeType.CutsceneGlobalMap))
		{
			return !Game.Instance.IsModeActive(GameModeType.Dialog);
		}
		return false;
	}

	private static bool CanShowSmallWindow(GameModeType gameMode)
	{
		if (!Game.Instance.IsModeActive(GameModeType.Cutscene))
		{
			return !Game.Instance.IsModeActive(GameModeType.CutsceneGlobalMap);
		}
		return false;
	}

	private static bool IsBigWindowTutorial([NotNull] TutorialData data)
	{
		return data.Blueprint.Windowed;
	}

	private bool IsBan(TutorialData data)
	{
		if (data.Trigger != null && Game.Instance.Player.Tutorial.Ensure(data.Blueprint).Banned)
		{
			return true;
		}
		if (Game.Instance.Player.Tutorial.IsTagBanned(data.Blueprint.Tag))
		{
			return true;
		}
		return false;
	}

	public void ShowTutorial(TutorialData data)
	{
		GameModeType currentMode = Game.Instance.CurrentMode;
		if ((IsBigWindowTutorial(data) && !CanShowBigWindow(currentMode)) || !CanShowSmallWindow(currentMode))
		{
			m_DataToRestore = data;
		}
		else
		{
			ShowTutorialInternal(data);
		}
	}

	private void ShowTutorialInternal(TutorialData data)
	{
		if (IsBan(data))
		{
			UberDebug.LogError("Tutorial is Banned");
			HideTutorial(data);
			return;
		}
		Game.Instance.Player.Tutorial.ShowingData = data;
		if (IsBigWindowTutorial(data))
		{
			TutorialModalWindowVM disposable = (BigWindowVM.Value = new TutorialModalWindowVM(data, delegate
			{
				DisposeAndRemove(BigWindowVM);
			}));
			AddDisposable(disposable);
		}
		else
		{
			TutorialHintWindowVM disposable2 = (SmallWindowVM.Value = new TutorialHintWindowVM(data, delegate
			{
				DisposeAndRemove(SmallWindowVM);
			}));
			AddDisposable(disposable2);
		}
	}

	public void HideTutorial(TutorialData data)
	{
		if (IsBigWindowTutorial(data))
		{
			DisposeAndRemove(BigWindowVM);
		}
		else
		{
			DisposeAndRemove(SmallWindowVM);
		}
		Game.Instance.Player.Tutorial.ShowingData = null;
	}
}
