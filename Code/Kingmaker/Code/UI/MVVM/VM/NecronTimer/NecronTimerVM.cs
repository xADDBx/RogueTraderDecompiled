using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.NecronTimer;

public class NecronTimerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IUnlockValueHandler, ISubscriber, IPartyCombatHandler, IUnlockHandler, IAreaLoadingStagesHandler
{
	private BlueprintUnlockableFlag m_NecronTimerUnlockableFlag;

	public readonly ReactiveProperty<bool> IsUnlockedAndVisible = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<int> CurrentTimerValue = new ReactiveProperty<int>();

	public readonly ReactiveCommand<int> UpdateTimer = new ReactiveCommand<int>();

	public NecronTimerVM()
	{
		m_NecronTimerUnlockableFlag = UIConfig.Instance.NecronTimer.TimeLoopBlueprintReference.Get();
		AddDisposable(EventBus.Subscribe(this));
		IsUnlockedAndVisible.Value = !m_NecronTimerUnlockableFlag.IsLocked && !Game.Instance.TurnController.InCombat;
		CurrentTimerValue.Value = m_NecronTimerUnlockableFlag.Value;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		if (flag == m_NecronTimerUnlockableFlag)
		{
			if (value < 0)
			{
				CurrentTimerValue.Value = 0;
				return;
			}
			if (value > UIConfig.Instance.NecronTimer.MaxTimerValue - 1)
			{
				CurrentTimerValue.Value = UIConfig.Instance.NecronTimer.MaxTimerValue - 1;
				return;
			}
			IsUnlockedAndVisible.Value = !flag.IsLocked && !Game.Instance.TurnController.InCombat;
			CurrentTimerValue.Value = value;
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		IsUnlockedAndVisible.Value = !m_NecronTimerUnlockableFlag.IsLocked && !inCombat;
	}

	public void HandleUnlock(BlueprintUnlockableFlag flag)
	{
		if (flag == m_NecronTimerUnlockableFlag)
		{
			IsUnlockedAndVisible.Value = !Game.Instance.TurnController.InCombat;
			CurrentTimerValue.Value = flag.Value;
		}
	}

	public void HandleLock(BlueprintUnlockableFlag flag)
	{
		if (flag == m_NecronTimerUnlockableFlag)
		{
			IsUnlockedAndVisible.Value = false;
		}
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		IsUnlockedAndVisible.Value = !m_NecronTimerUnlockableFlag.IsLocked && !Game.Instance.TurnController.InCombat;
		CurrentTimerValue.Value = m_NecronTimerUnlockableFlag.Value;
		UpdateTimer.Execute(CurrentTimerValue.Value);
	}
}
