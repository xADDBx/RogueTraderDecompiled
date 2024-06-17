using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UI.Dependencies;
using Owlcat.Runtime.UI.Utility;
using Rewired;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput;

public class GamePad
{
	[Serializable]
	public class ButtonDescription
	{
		public string SystemName;

		public Sprite Icon;
	}

	[Serializable]
	public class GamePadDescription
	{
		public List<ButtonDescription> Buttons;
	}

	private static GamePad s_Instance;

	private const string RewiredDualShock4Name = "Sony DualShock 4";

	private const string RewiredDualSense5Name = "Sony DualSense";

	private const string RewiredNintendoSwitchName = "Nintendo Switch Pro Controller";

	private const string RewiredSteamControllerName = "Steam Controller";

	public ReactiveCommand OnLayerPushed = new ReactiveCommand();

	public ReactiveCommand OnLayerPoped = new ReactiveCommand();

	public ReactiveProperty<ConsoleType> ConsoleTypeProperty = new ReactiveProperty<ConsoleType>();

	public ReactiveProperty<bool> IsSwitchController = new ReactiveProperty<bool>();

	private Player m_Player;

	private bool m_IsRunOnSteamDeck;

	private InputLayer m_BugReportLayer;

	private InputLayer m_BaseLayer;

	private InputLayer m_OverlayLayer;

	private List<InputLayer> m_Layers = new List<InputLayer>();

	public static GamePad Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new GamePad();
			}
			return s_Instance;
		}
	}

	public ConsoleType Type => ConsoleTypeProperty.Value;

	public Player Player
	{
		get
		{
			if (m_Player == null)
			{
				m_Player = ReInput.players?.GetPlayer(0);
			}
			return m_Player;
		}
	}

	public InputLayer BugReportLayer => m_BugReportLayer;

	public InputLayer BaseLayer => m_BaseLayer;

	public InputLayer OverlayLayer => m_OverlayLayer;

	public List<InputLayer> Layers => m_Layers;

	public bool CursorEnabled
	{
		get
		{
			InputLayer baseLayer = m_BaseLayer;
			if (baseLayer == null || !baseLayer.CursorEnabled)
			{
				if (m_Layers.Count > 0)
				{
					return m_Layers.Last()?.CursorEnabled ?? false;
				}
				return false;
			}
			return true;
		}
	}

	public bool SwapButtonsForJapanese { get; private set; }

	public InputLayer CurrentInputLayer
	{
		get
		{
			if (m_Layers.Count <= 0)
			{
				return m_BaseLayer;
			}
			return m_Layers.Last();
		}
	}

	private GamePad()
	{
		UpdateConsoleType();
		ReInput.ControllerConnectedEvent += delegate
		{
			UpdateConsoleType();
		};
		IsSwitchController.Subscribe(delegate
		{
			UpdateConsoleType();
		});
	}

	private void UpdateConsoleType()
	{
		SwapButtonsForJapanese = IsSwitchController.Value;
		if (IsSwitchController.Value)
		{
			ConsoleTypeProperty.Value = ConsoleType.Switch;
			return;
		}
		if (m_IsRunOnSteamDeck)
		{
			ConsoleTypeProperty.Value = ConsoleType.SteamDeck;
			return;
		}
		Joystick[] array = ReInput.controllers?.GetJoysticks();
		if (array == null || array.Length == 0 || array[0] == null)
		{
			ConsoleTypeProperty.Value = ConsoleType.XBox;
			return;
		}
		switch (array[0].name)
		{
		case "Sony DualShock 4":
			ConsoleTypeProperty.Value = ConsoleType.PS4;
			break;
		case "Sony DualSense":
			ConsoleTypeProperty.Value = ConsoleType.PS5;
			break;
		case "Nintendo Switch Pro Controller":
			ConsoleTypeProperty.Value = ConsoleType.Switch;
			break;
		case "Steam Controller":
			ConsoleTypeProperty.Value = ConsoleType.SteamController;
			break;
		default:
			ConsoleTypeProperty.Value = ConsoleType.XBox;
			break;
		}
	}

	public void SetIsRunOnSteamDeck(bool value)
	{
		m_IsRunOnSteamDeck = value;
		UpdateConsoleType();
	}

	public IDisposable SetBugReportLayer(InputLayer layer)
	{
		m_BugReportLayer?.Unbind();
		m_BugReportLayer = layer;
		m_BugReportLayer?.Bind();
		UILog.SetBugReportLayer(layer?.ContextName ?? "Empty");
		return Disposable.Create(delegate
		{
			m_BugReportLayer?.Unbind();
			m_BugReportLayer = null;
			UILog.SetBugReportLayer("Empty");
		});
	}

	public void SetBaseLayer(InputLayer layer)
	{
		m_BaseLayer?.Unbind();
		m_BaseLayer = layer;
		m_BaseLayer?.Bind();
		UILog.SetBaseLayer(layer?.ContextName ?? "Empty");
		TryRestoreCursorState();
	}

	public void SetOverlayLayer(InputLayer layer)
	{
		m_OverlayLayer?.Unbind();
		m_OverlayLayer = layer;
		m_OverlayLayer?.Bind();
		UILog.SetOverlayLayer(layer?.ContextName ?? "Empty");
	}

	public IDisposable PushLayer(InputLayer layer)
	{
		if (layer == null)
		{
			return Disposable.Empty;
		}
		if (m_Layers.Count > 0)
		{
			m_Layers.Last().Unbind();
		}
		UILog.PushLayer(layer.ContextName);
		m_Layers.Add(layer);
		layer.Bind();
		OnLayerPushed.Execute();
		TryRestoreCursorState();
		return Disposable.Create(delegate
		{
			PopLayer(layer);
		});
	}

	public bool PopLayer(InputLayer layer)
	{
		if (layer == null)
		{
			UIKitLogger.Error("PopLayer: Pop NULL InputLayer");
			return false;
		}
		if (!m_Layers.Contains(layer))
		{
			UIKitLogger.Error("PopLayer: No such layer in list of pushed layers!");
			return false;
		}
		if (m_Layers.Last() == layer)
		{
			m_Layers.Last().Unbind();
			m_Layers.Remove(layer);
			UILog.PopLayer(layer.ContextName);
			if (m_Layers.Count > 0)
			{
				m_Layers.Last().Bind();
			}
		}
		else
		{
			m_Layers.Remove(layer);
			UILog.RemoveLayer(layer.ContextName);
		}
		OnLayerPoped.Execute();
		TryRestoreCursorState();
		return true;
	}

	public void TryRestoreCursorState()
	{
		if (UIKitRewiredCursorController.HasController)
		{
			DoRestoreConfigState();
		}
	}

	private void DoRestoreConfigState()
	{
		bool active = (UIKitRewiredCursorController.Enabled = CursorEnabled);
		if (UIKitRewiredCursorController.HasCursor)
		{
			UIKitRewiredCursorController.Cursor.SetActive(active);
		}
	}

	public void Clear()
	{
		m_Layers.Clear();
		m_BaseLayer = null;
	}
}
