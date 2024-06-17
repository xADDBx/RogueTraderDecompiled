using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Pointer;
using Kingmaker.View;
using Kingmaker.Visual.LocalMap;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.Dependencies;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.LocalMap.Console;

public class LocalMapConsoleView : LocalMapBaseView
{
	[SerializeField]
	private float m_StickCameraMoveFactor = 30f;

	[SerializeField]
	private float m_StickCursorFactor = 2f;

	[SerializeField]
	private float m_StickMapDragFactor = 1f;

	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_OpenLegendHint;

	[SerializeField]
	private ConsoleHint m_HideLegendHint;

	private InputLayer m_InputLayer;

	private readonly BoolReactiveProperty m_LegendShowed = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_CursorActive = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_RotateCameraMode = new BoolReactiveProperty();

	private Vector2 m_CameraFollowerVector;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_CursorActive.Value = false;
		m_RotateCameraMode.Value = Game.Instance.Player.IsCameraRotateMode;
		AddInput();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		SwitchCursor(m_InputLayer, state: false);
	}

	public void AddInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "LocalMap",
			CursorEnabled = false
		};
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(m_OpenLegendHint.Bind(m_InputLayer.AddButton(delegate
		{
			InteractLocalMapHistory(state: true);
		}, 11, m_LegendShowed.Not().ToReactiveProperty())));
		AddDisposable(m_HideLegendHint.Bind(m_InputLayer.AddButton(delegate
		{
			InteractLocalMapHistory(state: false);
		}, 11, m_LegendShowed)));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			SwitchCameraMode(!m_RotateCameraMode.Value);
		}, 19), UIStrings.Instance.HUDTexts.SwitchCameraMode));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			SwitchCursor(m_InputLayer, !m_CursorActive.Value);
		}, 18), UIStrings.Instance.HUDTexts.Pointer));
		AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			FindRogueTrader(smooth: true);
		}, 10), UIStrings.Instance.LocalMapTexts.CenterOnRogueTrader));
		AddDisposable(m_InputLayer.AddAxis2D(OnLeftStickMove, 0, 1, repeat: false));
		AddDisposable(m_InputLayer.AddAxis2D(OnRightStickMove, 2, 3, repeat: false));
		AddDisposable(m_InputLayer.AddButton(delegate
		{
			ConfirmClick();
		}, 8));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private static void RotateCamera(bool direction)
	{
		CameraRig instance = CameraRig.Instance;
		if (direction)
		{
			instance.RotateRight();
		}
		else
		{
			instance.RotateLeft();
		}
	}

	private void SwitchCursor(InputLayer inputLayer, bool state)
	{
		if (inputLayer != null)
		{
			inputLayer.CursorEnabled = state;
			m_CursorActive.Value = state;
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleHintRequest(null, shouldShow: true);
			});
		}
	}

	private void SwitchCameraMode(bool state)
	{
		m_RotateCameraMode.Value = (Game.Instance.Player.IsCameraRotateMode = state);
		EventBus.RaiseEvent(delegate(ITooltipHandler h)
		{
			h.HandleHintRequest(null, shouldShow: true);
		});
	}

	private void InteractLocalMapHistory(bool state)
	{
		ShowLocalMapHistory(state);
		m_LegendShowed.Value = state;
	}

	private void OnLeftStickMove(InputActionEventData eventData, Vector2 vec)
	{
		if (m_CursorActive.Value)
		{
			ConsoleCursor.Instance.MoveCursor(vec / m_StickCursorFactor);
		}
		else
		{
			UpdateMapPosition(vec * CurrentZoom * (0f - m_StickMapDragFactor));
		}
	}

	private void OnRightStickMove(InputActionEventData eventData, Vector2 vec)
	{
		if (m_RotateCameraMode.Value)
		{
			if (vec.x < -0.5f)
			{
				RotateCamera(direction: true);
			}
			if (vec.x > 0.5f)
			{
				RotateCamera(direction: false);
			}
			float y = vec.y;
			if (y < -0.2f || y > 0.2f)
			{
				SetMapScale(vec.y / 5f);
			}
		}
		else
		{
			vec = base.ViewModel.LocalMapRotation switch
			{
				BlueprintAreaPart.LocalMapRotationDegree.Degree0 => new Vector2(vec.x, vec.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree90 => new Vector2(vec.y, 0f - vec.x), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree180 => new Vector2(0f - vec.x, 0f - vec.y), 
				BlueprintAreaPart.LocalMapRotationDegree.Degree270 => new Vector2(0f - vec.y, vec.x), 
				_ => vec, 
			};
			Game.Instance.CameraController.Follower.Release();
			Vector3 vector = WarhammerLocalMapRenderer.Instance.WorldToViewportPoint(CameraRig.Instance.transform.position);
			vector.x += vec.x / m_StickCameraMoveFactor;
			vector.y += vec.y / m_StickCameraMoveFactor;
			base.ViewModel.OnClick(vector, state: true, null, canPing: false);
			m_CameraFollowerVector = vector;
		}
	}

	private void ConfirmClick()
	{
		if (!m_CursorActive.Value)
		{
			base.ViewModel.OnClick(m_CameraFollowerVector, state: false);
			return;
		}
		Vector2 viewportPos = GetViewportPos(ConsoleCursor.Instance.Position);
		base.ViewModel.OnClick(viewportPos, state: false);
		base.ViewModel.OnClick(viewportPos, state: true);
		UIKitSoundManager.PlayButtonClickSound();
	}

	protected override void InteractableRightButtons()
	{
		base.InteractableRightButtons();
		Vector3 localScale = m_Image.rectTransform.localScale;
		MinZoom.Value = localScale.x > m_ZoomMinSize && localScale.y > m_ZoomMinSize;
		MaxZoom.Value = localScale.x < m_ZoomMaxSize && localScale.y < m_ZoomMaxSize;
	}

	private void Close()
	{
		SwitchCursor(m_InputLayer, state: false);
		EventBus.RaiseEvent(delegate(INewServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
	}
}
