using System;
using System.Collections.Generic;
using Owlcat.Runtime.UI.Dependencies;
using UnityEngine;

namespace Owlcat.Runtime.UI.MVVM;

public abstract class ViewBase<TViewModel> : MonoBehaviour, IHasViewModel where TViewModel : class, IViewModel
{
	private List<IDisposable> m_BindDisposable;

	protected TViewModel ViewModel { get; private set; }

	public bool IsBinded => ViewModel != null;

	protected void AddDisposable(IDisposable disposable)
	{
		if (disposable != null)
		{
			if (m_BindDisposable == null)
			{
				m_BindDisposable = new List<IDisposable>();
			}
			if (!m_BindDisposable.Contains(disposable))
			{
				m_BindDisposable.Add(disposable);
			}
		}
	}

	protected bool RemoveDisposable(IDisposable disposable)
	{
		if (disposable != null)
		{
			return m_BindDisposable.Remove(disposable);
		}
		return false;
	}

	public void Bind(TViewModel viewModel)
	{
		Unbind();
		ViewModel = viewModel;
		if (ViewModel == null)
		{
			return;
		}
		ViewModel.OnDispose += DestroyView;
		try
		{
			BindViewImplementation();
		}
		catch (Exception ex)
		{
			UIKitLogger.Exception("Bind Exception: " + ex);
		}
		finally
		{
		}
	}

	public void Unbind()
	{
		if (ViewModel == null)
		{
			return;
		}
		ViewModel.OnDispose -= DestroyView;
		if (m_BindDisposable != null && m_BindDisposable.Count > 0)
		{
			foreach (IDisposable item in m_BindDisposable)
			{
				try
				{
					item.Dispose();
				}
				catch (Exception ex)
				{
					UIKitLogger.Exception("DestroyView Exception: " + ex);
				}
			}
			m_BindDisposable.Clear();
		}
		try
		{
			DestroyViewImplementation();
		}
		catch (Exception ex2)
		{
			UIKitLogger.Exception("DestroyView Exception: " + ex2);
		}
		ViewModel = null;
	}

	protected void DestroyView()
	{
		Unbind();
	}

	public IViewModel GetViewModel()
	{
		return ViewModel;
	}

	protected abstract void BindViewImplementation();

	protected abstract void DestroyViewImplementation();
}
