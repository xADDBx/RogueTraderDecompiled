using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View.Surface;
using Kingmaker.Code.UI.MVVM.View.Surface.Console;
using Kingmaker.Code.UI.MVVM.View.Surface.PC;
using Kingmaker.GameModes;
using Kingmaker.UI.Models;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UniRx;
using Rewired;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Surface.Console;

public class SurfaceConsoleView : SurfaceBaseView
{
	[SerializeField]
	private SurfaceStaticPartConsoleView m_StaticPartConsoleView;

	[SerializeField]
	private SurfaceDynamicPartConsoleView m_DynamicPartConsoleView;

	[SerializeField]
	private SurfaceCombatPartView m_SurfaceCombatPartView;

	private static bool GamePadConfirm
	{
		set
		{
			if (Game.Instance.ClickEventsController != null)
			{
				Game.Instance.ClickEventsController.GamePadConfirm = value;
			}
		}
	}

	private static bool GamePadDecline
	{
		set
		{
			if (Game.Instance.ClickEventsController != null)
			{
				Game.Instance.ClickEventsController.GamePadDecline = value;
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_StaticPartConsoleView.Initialize();
		m_DynamicPartConsoleView.Initialize();
		m_SurfaceCombatPartView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_StaticPartConsoleView.Bind(base.ViewModel.StaticPartVM);
		m_DynamicPartConsoleView.Bind(base.ViewModel.DynamicPartVM);
		m_SurfaceCombatPartView.Bind(base.ViewModel.CombatPartVM);
		base.BindViewImplementation();
	}

	protected override void CreateBaseInputImpl(InputLayer baseInputLayer)
	{
		m_StaticPartConsoleView.AddBaseInput(baseInputLayer);
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			DelayedQuestNotificationDecline();
		}, 9, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			DelayedQuestNotificationJournal();
		}, 17));
	}

	protected override void CreateMainInputImpl(InputLayer mainInputLayer)
	{
		AddDisposable(SurfaceMainInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		AddDisposable(mainInputLayer.AddButton(OnInteract, 8, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(mainInputLayer.AddButton(OnDecline, 9, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(mainInputLayer.AddButton(delegate
		{
			OnNextInteractable(combat: false);
		}, 15, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(mainInputLayer.AddButton(delegate
		{
			OnPrevInteractable(combat: false);
		}, 14, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		m_StaticPartConsoleView.AddMainInput(mainInputLayer);
		SubscribeToPointerClicks(mainInputLayer);
	}

	protected override void CreateCombatInputImpl(InputLayer combatInputLayer)
	{
		AddDisposable(SurfaceCombatInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		m_StaticPartConsoleView.HandleBeginCombat();
		AddDisposable(combatInputLayer.AddButton(delegate
		{
			OnNextInteractable(combat: true);
		}, 15, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(combatInputLayer.AddButton(delegate
		{
			OnPrevInteractable(combat: true);
		}, 14, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		m_StaticPartConsoleView.AddCombatInput(combatInputLayer);
		SubscribeToPointerClicks(combatInputLayer);
	}

	private void SubscribeToPointerClicks(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadConfirm = true;
		}, 8));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadConfirm = false;
		}, 8, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadConfirm = false;
		}, 8, InputActionEventType.ButtonLongPressJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadDecline = true;
		}, 9));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadDecline = false;
		}, 9, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadDecline = false;
		}, 9, InputActionEventType.ButtonLongPressJustReleased));
	}

	private void OnInteract(InputActionEventData eventData)
	{
		SurfaceMainInputLayer.OnInteract();
	}

	private void DelayedQuestNotificationDecline()
	{
		DelayedInvoker.InvokeInFrames(OnQuestNotificationDecline, 1);
	}

	private void DelayedQuestNotificationJournal()
	{
		DelayedInvoker.InvokeInFrames(OnQuestNotificationJournal, 1);
	}

	private void OnQuestNotificationDecline()
	{
		if (base.ViewModel.IsInQuestNotification)
		{
			base.ViewModel.QuestNotificationForceClose();
		}
	}

	private void OnQuestNotificationJournal()
	{
		bool flag = RootUIContext.Instance.FullScreenUIType == FullScreenUIType.Journal;
		bool flag2 = Game.Instance.CurrentMode == GameModeType.Cutscene;
		bool flag3 = Game.Instance.CurrentMode == GameModeType.Dialog;
		if (base.ViewModel.IsInQuestNotification && !flag && !flag2 && !flag3)
		{
			base.ViewModel.OpenJournal();
		}
	}

	private void OnDecline(InputActionEventData obj)
	{
		_ = base.ViewModel.IsInQuestNotification;
	}

	private void OnNextInteractable(bool combat)
	{
		if (combat)
		{
			SurfaceCombatInputLayer.OnNextInteractable();
		}
		else
		{
			SurfaceMainInputLayer.OnNextInteractable();
		}
	}

	private void OnPrevInteractable(bool combat)
	{
		if (combat)
		{
			SurfaceCombatInputLayer.OnPrevInteractable();
		}
		else
		{
			SurfaceMainInputLayer.OnPrevInteractable();
		}
	}

	private void OnShowEscMenu()
	{
		if (!Game.Instance.Player.Tutorial.HasShownData && !(Game.Instance.CurrentMode == GameModeType.GameOver))
		{
			m_StaticPartConsoleView.OnShowEscMenu();
		}
	}
}
