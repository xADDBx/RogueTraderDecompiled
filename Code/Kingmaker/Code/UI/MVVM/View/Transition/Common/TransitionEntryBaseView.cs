using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Code.UI.MVVM.VM.Transition;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Transition.Common;

public class TransitionEntryBaseView : ViewBase<TransitionEntryVM>
{
	[SerializeField]
	private BlueprintMultiEntranceEntry.Reference m_EntranceEntry;

	[SerializeField]
	private OwlcatMultiButton m_MapButton;

	private IDisposable m_HintDisposable;

	public BlueprintMultiEntranceEntry EntranceEntry => m_EntranceEntry;

	public void Initialize()
	{
		m_MapButton.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsVisible.CombineLatest(base.ViewModel.IsInteractable, (bool isVisible, bool isInteractable) => new { isVisible, isInteractable }).Subscribe(value =>
		{
			CheckEntriesEnabled(value.isVisible, value.isInteractable);
		}));
		AddDisposable(m_MapButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Enter();
		}));
		AddDisposable(m_MapButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Enter();
		}));
		CheckEntriesEnabled(base.ViewModel.IsVisible.Value, base.ViewModel.IsInteractable.Value);
	}

	protected override void DestroyViewImplementation()
	{
		m_HintDisposable?.Dispose();
		m_HintDisposable = null;
	}

	private void CheckEntriesEnabled(bool isVisible, bool isInteractable)
	{
		m_HintDisposable?.Dispose();
		m_MapButton.gameObject.SetActive(isVisible);
		if (isVisible)
		{
			m_MapButton.SetInteractable(isInteractable);
			if (!isInteractable)
			{
				m_HintDisposable = m_MapButton.SetHint(UIStrings.Instance.Transition.TransitionIsUnavailable);
			}
		}
	}
}
