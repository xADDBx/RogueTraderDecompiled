using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput.ConsoleTypeProviders;
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

	public readonly ReactiveCommand OnLayerPushed = new ReactiveCommand();

	public readonly ReactiveCommand OnLayerPoped = new ReactiveCommand();

	public readonly ReactiveProperty<ConsoleType> ConsoleTypeProperty = new ReactiveProperty<ConsoleType>();

	public bool Switch2MouseModeActive;

	private readonly OverriddenPlatformProvider m_OverriddenPlatform = new OverriddenPlatformProvider();

	private readonly List<ConsoleTypeProvider> m_TypeProviders;

	private Player m_Player;

	private InputLayer m_BugReportLayer;

	private InputLayer m_BaseLayer;

	private InputLayer m_OverlayLayer;

	private List<InputLayer> m_Layers = new List<InputLayer>();

	public static GamePad Instance => s_Instance ?? (s_Instance = new GamePad());

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
		m_TypeProviders = new List<ConsoleTypeProvider>
		{
			m_OverriddenPlatform,
			new PlatformTypeProvider(),
			new RewiredTypeProvider(),
			new DefaultTypeProvider()
		};
		UpdateConsoleType();
		ReInput.ControllerConnectedEvent += delegate
		{
			UpdateConsoleType();
		};
	}

	private void UpdateConsoleType()
	{
		foreach (ConsoleTypeProvider typeProvider in m_TypeProviders)
		{
			if (typeProvider.TryGetConsoleType(out var type))
			{
				ConsoleTypeProperty.Value = type;
				break;
			}
		}
		if (ConsoleTypeProperty.Value == ConsoleType.Switch)
		{
			SwapButtonsForJapanese = true;
		}
	}

	public void SetOverriddenConsoleType(ConsoleType type)
	{
		m_OverriddenPlatform.SetConsoleType(type);
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
		if (Switch2Helper.IsRunningOnSwitch2 && Switch2MouseModeActive)
		{
			DoRestoreConfigState();
		}
		else if (UIKitRewiredCursorController.HasController)
		{
			DoRestoreConfigState();
		}
	}

	private void DoRestoreConfigState()
	{
		bool flag = CursorEnabled;
		if (Switch2Helper.IsRunningOnSwitch2)
		{
			flag = Switch2MouseModeActive || CursorEnabled;
		}
		UIKitRewiredCursorController.Enabled = flag;
		if (UIKitRewiredCursorController.HasCursor)
		{
			UIKitRewiredCursorController.Cursor.SetActive(flag);
		}
	}

	public void Clear()
	{
		m_Layers.Clear();
		m_BaseLayer = null;
	}
}
