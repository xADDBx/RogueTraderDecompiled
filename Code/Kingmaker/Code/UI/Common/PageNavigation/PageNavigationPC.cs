using System;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.PageNavigation;

public class PageNavigationPC : PageNavigationBase
{
	[Header("PC")]
	[SerializeField]
	private OwlcatMultiButton m_PreviousButton;

	[SerializeField]
	private OwlcatMultiButton m_NextButton;

	public override void Initialize(int pageCount, IntReactiveProperty pageIndex, Action prevCallback = null, Action nextCallback = null)
	{
		base.Initialize(pageCount, pageIndex, prevCallback, nextCallback);
		Disposables.Add(m_PreviousButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnPreviousClick();
		}));
		Disposables.Add(m_NextButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnNextClick();
		}));
		OnCurrentIndexChanged(pageIndex.Value);
	}

	protected override void OnCurrentIndexChanged(int index)
	{
		base.OnCurrentIndexChanged(index);
		m_PreviousButton.Interactable = base.HasPrevious;
		m_NextButton.Interactable = base.HasNext;
	}
}
