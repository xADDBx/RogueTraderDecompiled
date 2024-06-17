using Kingmaker.Code.UI.MVVM.VM.SectorMap;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SectorMap;

public class SpaceFiltersPCView : ViewBase<SpaceFiltersVM>
{
	[Header("Buttons")]
	[SerializeField]
	private OwlcatMultiButton m_NamesFilterButton;

	[SerializeField]
	private OwlcatMultiButton m_RoutesFilterButton;

	[SerializeField]
	private OwlcatMultiButton m_RumorsFilterButton;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.SystemsVisible.AsObservable().Subscribe(delegate(bool value)
		{
			m_NamesFilterButton.SetActiveLayer((!value) ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.RoutesVisible.AsObservable().Subscribe(delegate(bool value)
		{
			m_RoutesFilterButton.SetActiveLayer((!value) ? 1 : 0);
		}));
		AddDisposable(base.ViewModel.RumorsVisible.AsObservable().Subscribe(delegate(bool value)
		{
			m_RumorsFilterButton.SetActiveLayer((!value) ? 1 : 0);
		}));
		AddDisposable(m_NamesFilterButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SwitchNamesVisible();
		}));
		AddDisposable(m_RoutesFilterButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SwitchRoutesVisible();
		}));
		AddDisposable(m_RumorsFilterButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SwitchRumorsVisible();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
