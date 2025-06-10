using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.InputSystems;

public class EscHotkeyManager : IService, IDisposable
{
	public const string KeyBindingName = "EscPressed";

	private readonly List<Action> m_EscSequence = new List<Action>();

	private KeyboardAccess m_Keyboard;

	public static EscHotkeyManager Instance => Services.GetInstance<EscHotkeyManager>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public void Dispose()
	{
		if (Application.isPlaying)
		{
			ClearKeyboardBind();
		}
	}

	private void ClearKeyboardBind()
	{
		m_Keyboard.Unbind("EscPressed", OnEscPressed);
		m_Keyboard = null;
	}

	public void Initialize()
	{
		if (m_Keyboard != null)
		{
			ClearKeyboardBind();
		}
		m_Keyboard = Game.Instance.Keyboard;
		DelayedInvoker.InvokeInFrames(delegate
		{
			Game.Instance.Keyboard.Bind("EscPressed", OnEscPressed);
		}, 1);
	}

	private void OnEscPressed()
	{
		if (m_EscSequence.Count >= 1)
		{
			List<Action> escSequence = m_EscSequence;
			escSequence[escSequence.Count - 1]();
		}
	}

	public IDisposable Subscribe(Action callback)
	{
		if (!m_EscSequence.Contains(callback))
		{
			m_EscSequence.Add(callback);
		}
		return Disposable.Create(delegate
		{
			Unsubscribe(callback);
		});
	}

	public void Unsubscribe(Action callback)
	{
		int num = m_EscSequence.LastIndexOf(callback);
		if (num >= 0)
		{
			m_EscSequence.RemoveAt(num);
		}
	}

	public bool HasCallback(Action callback)
	{
		return m_EscSequence.LastIndexOf(callback) >= 0;
	}
}
