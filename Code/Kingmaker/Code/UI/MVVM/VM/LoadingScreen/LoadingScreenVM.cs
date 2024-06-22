using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.LoadingScreen;

public class LoadingScreenVM : VMBase, IStartAwaitingUserInput, ISubscriber, IContinueLoadingHandler, INetEvents
{
	public readonly ReactiveProperty<BlueprintArea> AreaProperty = new ReactiveProperty<BlueprintArea>();

	public LoadingScreenState State;

	private int m_RandomScreenPercent;

	public readonly BoolReactiveProperty NeedUserInput = new BoolReactiveProperty(initialValue: false);

	public readonly IntReactiveProperty UserInputProgress = new IntReactiveProperty(0);

	public readonly IntReactiveProperty UserInputTarget = new IntReactiveProperty(1);

	public readonly BoolReactiveProperty UserInputMeIsPressed = new BoolReactiveProperty(initialValue: false);

	private bool m_UpdateProgressInput;

	public readonly BoolReactiveProperty IsSaveTransfer = new BoolReactiveProperty(initialValue: false);

	public readonly IntReactiveProperty SaveTransferProgress = new IntReactiveProperty(0);

	public readonly IntReactiveProperty SaveTransferTarget = new IntReactiveProperty(1);

	private BlueprintArea m_LastArea;

	public StatefulRandom Random
	{
		get
		{
			if (LoadingProcess.Instance.CurrentProcessTag == LoadingProcessTag.ExceptionReporter || LoadingProcess.Instance.CurrentProcessTag == LoadingProcessTag.ResetUI)
			{
				return PFStatefulRandom.NonDeterministic;
			}
			return PFStatefulRandom.LoadingScreen;
		}
	}

	public float FontMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public LoadingScreenVM(BlueprintArea area)
	{
		AddDisposable(EventBus.Subscribe(this));
		SetLoadingArea(area);
		IsSaveTransfer.Value = PhotonManager.Save.InProcess;
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
		{
			SaveTransferUpdate();
			UpdateLockProgress();
			NeedUserInput.Value = LoadingProcess.Instance.IsAwaitingUserInput;
		}));
	}

	public void SetLoadingArea(BlueprintArea area)
	{
		m_LastArea = Game.Instance.CurrentlyLoadedArea;
		AreaProperty.Value = area;
	}

	protected override void DisposeImplementation()
	{
		LoadingProcess.Instance.IsAwaitingUserInput.Release();
	}

	public int RandomLoadingScreen(int items, int[] itemsPercent)
	{
		if (items != itemsPercent.Length - 1)
		{
			return 0;
		}
		m_RandomScreenPercent = Random.Range(1, 100);
		int num = itemsPercent[0];
		for (int i = 0; i <= items; i++)
		{
			bool num2 = m_RandomScreenPercent >= num + 1 && m_RandomScreenPercent <= num + itemsPercent[i + 1];
			num += itemsPercent[i + 1];
			if (num2)
			{
				return i;
			}
		}
		return 0;
	}

	public void OnStartAwaitingUserInput()
	{
		LoadingProcess.Instance.IsAwaitingUserInput.Retain();
		if (AreaProperty.Value == null || AreaProperty.Value.NotPause || AreaProperty.Value == m_LastArea || !SettingsRoot.Game.Autopause.PauseOnLoadingScreen)
		{
			LoadingProcess.Instance.IsAwaitingUserInput.Release();
		}
	}

	void IContinueLoadingHandler.HandleContinueLoading()
	{
		LoadingProcess.Instance.IsAwaitingUserInput.Release();
		m_UpdateProgressInput = true;
	}

	public void HandleTransferProgressChanged(bool value)
	{
		IsSaveTransfer.Value = value;
		SaveTransferUpdate();
	}

	public void HandleNetStateChanged(LobbyNetManager.State state)
	{
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
	}

	public void HandleNLoadingScreenClosed()
	{
	}

	private void SaveTransferUpdate()
	{
		if (IsSaveTransfer.Value && PhotonManager.Save.GetSentProgress(out var progress, out var target))
		{
			SaveTransferProgress.Value = progress;
			SaveTransferTarget.Value = target;
		}
	}

	private void UpdateLockProgress()
	{
		if (m_UpdateProgressInput && PhotonManager.Lock.GetProgress(NetLockPointId.LoadingProcess, out var current, out var target, out var me))
		{
			UserInputProgress.Value = current;
			UserInputTarget.Value = target;
			UserInputMeIsPressed.Value = me;
		}
	}
}
