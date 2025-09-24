using System;
using System.Linq;
using DG.Tweening;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Fade;

public class FadeVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ILoadingScreen, IGameModeHandler, ISubscriber
{
	public struct AdvancedParams
	{
		public Ease Ease;

		public float Duration;
	}

	public struct Params
	{
		public bool Fade;

		public AdvancedParams? FadeParams;
	}

	private LoadingScreenState m_State;

	public readonly BoolReactiveProperty CutsceneOverlay = new BoolReactiveProperty();

	public readonly ReactiveProperty<Params> LoadingScreen = new ReactiveProperty<Params>();

	private AdvancedParams? FadeParams;

	public FadeVM()
	{
		FadeCanvas.Instance = this;
		EventBus.Subscribe(this);
		DoCutScene(Game.Instance.CurrentMode == GameModeType.Cutscene);
	}

	public void ShowLoadingScreen()
	{
		if (m_State != LoadingScreenState.Shown && m_State != LoadingScreenState.ShowAnimation)
		{
			PFLog.UI.Log("Show fade");
			m_State = LoadingScreenState.ShowAnimation;
			LoadingScreen.Value = new Params
			{
				Fade = true,
				FadeParams = FadeParams
			};
		}
	}

	public void HideLoadingScreen()
	{
		if (m_State == LoadingScreenState.Hidden || m_State == LoadingScreenState.HideAnimation)
		{
			return;
		}
		PFLog.UI.Log("Hide fade");
		m_State = LoadingScreenState.HideAnimation;
		LoadingScreen.Value = new Params
		{
			Fade = false,
			FadeParams = FadeParams
		};
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			EventBus.RaiseEvent(delegate(ISystemMapRadarHandler h)
			{
				h.HandleShowSystemMapRadar();
			});
		}
	}

	public LoadingScreenState GetLoadingScreenState()
	{
		return m_State;
	}

	protected override void DisposeImplementation()
	{
		EventBus.Unsubscribe(this);
		if (FadeCanvas.Instance == this)
		{
			FadeCanvas.Instance = null;
		}
	}

	public void Fadeout(bool fade)
	{
		Fadeout(fade, null);
	}

	public void Fadeout(bool fade, float duration, Ease ease)
	{
		Fadeout(fade, new AdvancedParams
		{
			Ease = ease,
			Duration = duration
		});
	}

	private void Fadeout(bool fade, AdvancedParams? fadeParams)
	{
		FadeParams = fadeParams;
		if (fade)
		{
			LoadingProcess.Instance.ShowManualLoadingScreen(this);
		}
		else
		{
			LoadingProcess.Instance.HideManualLoadingScreen();
		}
	}

	private void DoCutScene(bool state)
	{
		bool flag = Game.Instance.State.Cutscenes.Any((CutscenePlayerData c) => c.Cutscene.LockControl && c.Cutscene.ShowOverlay);
		CutsceneOverlay.Value = state && flag;
	}

	public void SetStateShowAnimation()
	{
		m_State = LoadingScreenState.ShowAnimation;
	}

	public void SetStateShown()
	{
		m_State = LoadingScreenState.Shown;
	}

	public void SetStateHideAnimation()
	{
		m_State = LoadingScreenState.HideAnimation;
	}

	public void SetStateHidden()
	{
		m_State = LoadingScreenState.Hidden;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			DoCutScene(state: true);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			DoCutScene(state: false);
		}
	}
}
