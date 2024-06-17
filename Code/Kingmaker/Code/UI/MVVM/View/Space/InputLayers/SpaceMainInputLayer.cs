using System;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Models;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Space.InputLayers;

public class SpaceMainInputLayer : InputLayer, IDisposable, IAbilityTargetSelectionUIHandler, ISubscriber
{
	private IDisposable m_FrameUpdate;

	protected Vector2 m_LeftStickVector;

	protected Vector2 m_RightStickVector;

	private InputBindStruct m_InputLeftStickBind;

	private InputBindStruct m_InputRightStickBind;

	public SpaceMainInputLayer()
	{
		EventBus.Subscribe(this);
		m_InputLeftStickBind = AddAxis2D(OnMoveLeftStick, 0, 1, repeat: false);
		m_InputRightStickBind = AddAxis2D(OnMoveRightStick, 2, 3, repeat: false);
		m_FrameUpdate = MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			OnUpdate();
		});
	}

	public virtual void Dispose()
	{
		base.CursorEnabled = false;
		m_FrameUpdate?.Dispose();
		m_FrameUpdate = null;
		m_InputLeftStickBind?.Dispose();
		m_InputLeftStickBind = null;
		m_InputRightStickBind?.Dispose();
		m_InputRightStickBind = null;
		EventBus.Unsubscribe(this);
	}

	private void OnMoveLeftStick(InputActionEventData eventData, Vector2 vec)
	{
		m_LeftStickVector = vec;
	}

	private void OnMoveRightStick(InputActionEventData eventData, Vector2 vec)
	{
		if (!CutsceneLock.Active)
		{
			m_RightStickVector = vec;
		}
	}

	private void OnUpdate()
	{
		if (LayerBinded.Value)
		{
			if (m_LeftStickVector != Vector2.zero)
			{
				UpdateCursorMovement();
			}
			if (m_RightStickVector != Vector2.zero)
			{
				UpdateCameraMovement();
			}
			m_LeftStickVector = (m_RightStickVector = Vector2.zero);
		}
	}

	protected virtual void UpdateCursorMovement()
	{
		ConsoleCursor.Instance.MoveCursor(m_LeftStickVector);
	}

	protected virtual void UpdateCameraMovement()
	{
		CameraRig instance = CameraRig.Instance;
		if (!Game.Instance.Player.IsCameraRotateMode)
		{
			Game.Instance.CameraController.Follower.Release();
			instance.ScrollBy2D(m_RightStickVector);
			return;
		}
		if (Mathf.Abs(m_RightStickVector.y) > 0.8f)
		{
			instance.CameraZoom.GamepadScrollPosition = m_RightStickVector.y;
		}
		if (m_RightStickVector.x < -0.5f)
		{
			instance.RotateRight();
		}
		if (m_RightStickVector.x > 0.5f)
		{
			instance.RotateLeft();
		}
	}

	public void SwitchCursorEnabled()
	{
		base.CursorEnabled = !base.CursorEnabled;
	}

	public void StopMovement()
	{
		m_LeftStickVector = Vector3.zero;
		SelectionManagerConsole.Instance.Or(null)?.Stop();
	}

	protected override void SetupCursor(bool value)
	{
		Game.Instance.CursorController.SetActive(value);
		base.SetupCursor(value);
		TooltipHelper.HideInfo();
		TooltipHelper.HideTooltip();
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
	}
}
