using Kingmaker.GameModes;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UniRx;
using Rewired;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Space.Console;

public class SpaceConsoleView : SpaceBaseView
{
	[FormerlySerializedAs("m_StaticPartPCView")]
	[SerializeField]
	private SpaceStaticPartConsoleView m_StaticPartConsoleView;

	[FormerlySerializedAs("m_DynamicPartPCView")]
	[SerializeField]
	private SpaceDynamicPartConsoleView m_DynamicPartConsoleView;

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
	}

	protected override void BindViewImplementation()
	{
		m_StaticPartConsoleView.Bind(base.ViewModel.StaticPartVM);
		m_DynamicPartConsoleView.Bind(base.ViewModel.DynamicPartVM);
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

	protected override void CreateCombatInputImpl(InputLayer combatInputLayer)
	{
		AddDisposable(SpaceCombatInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		combatInputLayer.AddButton(EndTurn, 9, InputActionEventType.ButtonJustLongPressed);
		Game.Instance.Player.IsCameraRotateMode = true;
		m_StaticPartConsoleView.AddCombatInput(combatInputLayer);
		SubscribeToPointerConfirmClicks(combatInputLayer);
		SubscribeToPointerDeclineClicks(combatInputLayer);
	}

	protected override void CreateSystemMapInputImpl(InputLayer systemMapInputLayer)
	{
		AddDisposable(SpaceSystemMapInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		systemMapInputLayer.AddButton(SwitchCursor, 18, InputActionEventType.ButtonJustReleased);
		m_StaticPartConsoleView.AddSystemMapInput(systemMapInputLayer);
		SubscribeToPointerConfirmClicks(systemMapInputLayer);
	}

	protected override void CreateGlobalMapInputImpl(InputLayer globalMapInputLayer)
	{
		AddDisposable(SpaceGlobalMapInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		Game.Instance.Player.IsCameraRotateMode = false;
		m_StaticPartConsoleView.AddGlobalMapInput(globalMapInputLayer);
		SubscribeToPointerConfirmClicks(globalMapInputLayer);
		SubscribeToPointerDeclineClicks(globalMapInputLayer);
	}

	private void SubscribeToPointerConfirmClicks(InputLayer inputLayer)
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
	}

	private void SubscribeToPointerDeclineClicks(InputLayer inputLayer)
	{
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

	private void EndTurn(InputActionEventData eventData)
	{
		UISounds.Instance.Sounds.Combat.EndTurn.Play();
		Game.Instance.TurnController.TryEndPlayerTurnManually();
	}

	private void SwitchCursor(InputActionEventData eventData)
	{
		SpaceSystemMapInputLayer.CursorEnabled = !SpaceSystemMapInputLayer.CursorEnabled;
		m_StaticPartConsoleView.SwitchSystemMapCursor(SpaceSystemMapInputLayer.CursorEnabled);
	}

	private void OnShowEscMenu()
	{
		if (!(Game.Instance.CurrentMode == GameModeType.GameOver))
		{
			m_StaticPartConsoleView.OnShowEscMenu();
		}
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
}
