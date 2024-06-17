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

	public BlueprintMultiEntranceEntry EntranceEntry => m_EntranceEntry;

	public void Initialize()
	{
		m_MapButton.gameObject.SetActive(value: false);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.IsVisible.CombineLatest(base.ViewModel.IsInteractable, (bool isVisible, bool isInteractable) => new { isVisible, isInteractable }).Subscribe(value =>
		{
			CheckEntriesEnabled();
		}));
		AddDisposable(m_MapButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Enter();
		}));
		AddDisposable(m_MapButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Enter();
		}));
		CheckEntriesEnabled();
	}

	private void CheckEntriesEnabled()
	{
		m_MapButton.gameObject.SetActive(base.ViewModel.IsVisible.Value);
		if (base.ViewModel.IsVisible.Value)
		{
			m_MapButton.SetInteractable(base.ViewModel.IsInteractable.Value);
		}
	}

	protected override void DestroyViewImplementation()
	{
	}
}
