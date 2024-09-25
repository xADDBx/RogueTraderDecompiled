using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.Dependencies;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.MVVM;

public abstract class BaseDisposable : IBaseDisposable, IDisposable
{
	private bool m_IsDisposed;

	private BaseDisposable m_Owner;

	private readonly List<IDisposable> m_Disposables = new List<IDisposable>();

	private Action m_OnDispose;

	private BaseDisposable Owner
	{
		get
		{
			return m_Owner;
		}
		set
		{
			if (m_Owner != null && value != null)
			{
				Debug.LogError($"Trying to assign a new owner to BaseDisposable that already have one. Old owner: {m_Owner}, new owner: {value}. Old owner will be kept.");
			}
			else
			{
				m_Owner = value;
			}
		}
	}

	public bool IsDisposed => m_IsDisposed;

	public event Action OnDispose
	{
		add
		{
			m_OnDispose = (Action)Delegate.Combine(m_OnDispose, value);
		}
		remove
		{
			m_OnDispose = (Action)Delegate.Remove(m_OnDispose, value);
		}
	}

	protected void AddDisposable(IDisposable disposable)
	{
		if (disposable != null && !m_Disposables.Contains(disposable))
		{
			m_Disposables.Add(disposable);
			if (disposable is BaseDisposable baseDisposable)
			{
				baseDisposable.Owner = this;
			}
		}
	}

	protected T AddDisposableAndReturn<T>(T disposable) where T : IDisposable
	{
		AddDisposable(disposable);
		return disposable;
	}

	protected void AddDisposable(Action action)
	{
		IDisposable d = Disposable.Create(action);
		AddDisposable(DisposableLeakDetection.Wrap(d));
	}

	protected bool RemoveDisposable(IDisposable disposable)
	{
		if (disposable is BaseDisposable baseDisposable)
		{
			baseDisposable.Owner = null;
		}
		if (disposable != null)
		{
			return m_Disposables.Remove(disposable);
		}
		return false;
	}

	protected bool DisposeAndRemove<T>(IReactiveProperty<T> disposableInProperty) where T : class, IDisposable
	{
		if (disposableInProperty.Value == null)
		{
			return false;
		}
		disposableInProperty.Value.Dispose();
		bool result = RemoveDisposable(disposableInProperty.Value);
		disposableInProperty.Value = null;
		return result;
	}

	protected bool DisposeAndRemove(ref IDisposable disposable)
	{
		disposable?.Dispose();
		bool result = RemoveDisposable(disposable);
		disposable = null;
		return result;
	}

	protected void DisposeAndRemoveMany<T>(ICollection<T> list) where T : class, IDisposable
	{
		foreach (T item in list)
		{
			if (item != null)
			{
				item.Dispose();
				RemoveDisposable(item);
			}
		}
		list.Clear();
	}

	protected abstract void DisposeImplementation();

	public BaseDisposable()
	{
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		try
		{
			m_OnDispose?.Invoke();
		}
		catch (Exception ex)
		{
			UIKitLogger.Exception("Error in Dispose: " + ex);
		}
		if (Owner != null)
		{
			Owner.RemoveDisposable(this);
		}
		m_OnDispose = null;
		if (m_Disposables != null && m_Disposables.Count > 0)
		{
			for (int num = m_Disposables.Count - 1; num >= 0; num--)
			{
				try
				{
					if (m_Disposables[num] is BaseDisposable baseDisposable)
					{
						baseDisposable.Owner = null;
					}
					m_Disposables[num].Dispose();
				}
				catch (Exception ex2)
				{
					UIKitLogger.Exception("Error in Dispose: " + ex2);
				}
			}
			m_Disposables.Clear();
		}
		DisposeImplementation();
	}

	public void DestroyViewRecursive()
	{
		DestroyView();
		if (m_Disposables == null || m_Disposables.Count <= 0)
		{
			return;
		}
		foreach (IDisposable disposable in m_Disposables)
		{
			try
			{
				if (disposable is BaseDisposable baseDisposable)
				{
					baseDisposable.DestroyView();
				}
			}
			catch (Exception ex)
			{
				UIKitLogger.Exception("Error in DestroyView: " + ex);
			}
		}
	}

	public void DestroyView()
	{
		try
		{
			m_OnDispose?.Invoke();
		}
		catch (Exception ex)
		{
			UIKitLogger.Exception("Error in DestroyView: " + ex);
		}
		m_OnDispose = null;
	}

	public static void LogAllUndisposed(ILogger logger)
	{
	}
}
