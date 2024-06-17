using System;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;

public class GamepadConnectDisconnectVM : VMBase
{
	public readonly ReactiveCommand GamepadConnected = new ReactiveCommand();

	public readonly ReactiveCommand GamepadDisconnected = new ReactiveCommand();

	private readonly Action<Game.ControllerModeType> m_ChangeControllerModeAction;

	private readonly Action m_ContinueGameStartRoutine;

	public static bool GamepadIsConnected
	{
		get
		{
			if (ReInput.isReady && ReInput.controllers != null)
			{
				return ReInput.controllers.joystickCount > 0;
			}
			return false;
		}
	}

	public Game.ControllerModeType? ControllerOverride => Game.ControllerOverride;

	public bool IsControllerOverride => ControllerOverride.HasValue;

	public GamepadConnectDisconnectVM(Action<Game.ControllerModeType> changeControllerModeAction, Action continueGameStartRoutine)
		: this()
	{
		m_ChangeControllerModeAction = changeControllerModeAction;
		m_ContinueGameStartRoutine = continueGameStartRoutine;
	}

	public GamepadConnectDisconnectVM()
	{
		if (!IsControllerOverride)
		{
			AddDisposable(GamepadConnected);
			AddDisposable(GamepadDisconnected);
			ReInput.ControllerConnectedEvent += OnGamepadConnected;
			ReInput.ControllerDisconnectedEvent += OnGamepadDisconnected;
			AddDisposable(delegate
			{
				ReInput.ControllerConnectedEvent -= OnGamepadConnected;
			});
			AddDisposable(delegate
			{
				ReInput.ControllerDisconnectedEvent -= OnGamepadDisconnected;
			});
		}
	}

	private void OnGamepadConnected(ControllerStatusChangedEventArgs obj)
	{
		GamepadConnected.Execute();
	}

	private void OnGamepadDisconnected(ControllerStatusChangedEventArgs obj)
	{
		GamepadDisconnected.Execute();
	}

	public void SetGamepadMode()
	{
		if (EventSystem.current != null)
		{
			UnityEngine.Object.Destroy(EventSystem.current.gameObject);
		}
		EventSystem.current = null;
		m_ChangeControllerModeAction?.Invoke(Game.ControllerModeType.Gamepad);
		m_ContinueGameStartRoutine?.Invoke();
	}

	public void SetKeyboardMode()
	{
		m_ChangeControllerModeAction?.Invoke(Game.ControllerModeType.Mouse);
		m_ContinueGameStartRoutine?.Invoke();
	}

	public void SwitchControlMode()
	{
		Game.Instance.ControllerMode = ((Game.Instance.ControllerMode != Game.ControllerModeType.Gamepad) ? Game.ControllerModeType.Gamepad : Game.ControllerModeType.Mouse);
		Game.DontChangeController = true;
		NetGame.State currentState = PhotonManager.NetGame.CurrentState;
		Game.ResetUI((currentState == NetGame.State.NetInitialized || currentState == NetGame.State.InLobby) ? ((Action)delegate
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				EventBus.RaiseEvent(delegate(INetLobbyRequest h)
				{
					h.HandleNetLobbyRequest();
				});
			}, 5);
		}) : null);
	}

	protected override void DisposeImplementation()
	{
	}

	public void DeclineController()
	{
		Game.Instance.ControllerMode = Game.ControllerModeType.Mouse;
		Game.DontChangeController = true;
	}
}
