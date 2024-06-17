using System;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.Common.PageNavigation;

public class PageNavigationPoint : MonoBehaviour, IDisposable
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	private CompositeDisposable m_Disposables;

	private Action m_SelectAction;

	private bool m_IsSelected;

	public void Initialize(Action selectAction)
	{
		m_SelectAction = selectAction;
		m_Disposables = new CompositeDisposable();
		m_Disposables.Add(m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			m_SelectAction?.Invoke();
		}));
	}

	public void SetSelected(bool value)
	{
		if (value != m_IsSelected)
		{
			m_IsSelected = value;
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
		}
	}

	public void Dispose()
	{
		m_Disposables?.Clear();
		m_Disposables = null;
	}
}
