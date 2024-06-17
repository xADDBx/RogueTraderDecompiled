using Kingmaker.Code.UI.MVVM.VM.Other.NestedSelectionGroup;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Other.NestedSelectionGroup;

public class NestedSelectionGroupEntityPCView<TViewModel> : VirtualListElementViewBase<TViewModel> where TViewModel : NestedSelectionGroupEntityVM
{
	[Header("Add to button layers: Collapsed | Expanded")]
	[SerializeField]
	protected OwlcatMultiButton m_CollapseButton;

	private bool m_IsInit;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			ClearView();
			DoInitialize();
			m_IsInit = true;
		}
	}

	public virtual void DoInitialize()
	{
	}

	public void ViewBind(IViewModel viewModel)
	{
		BindViewImplementation();
	}

	public void DestroyViewItem()
	{
		DestroyViewImplementation();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			OnClick();
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.RefreshView.ObserveLastValueOnLateUpdate(), delegate
		{
			RefreshView();
		}));
		RefreshView();
		AddDisposable(base.ViewModel.IsSelected.ObserveLastValueOnLateUpdate().Subscribe(OnChangeSelectedState));
		AddDisposable(base.ViewModel.IsAvailable.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.Value);
		}));
		if (!m_CollapseButton)
		{
			return;
		}
		if (base.ViewModel.HasNesting)
		{
			m_CollapseButton.gameObject.SetActive(value: true);
			AddDisposable(ObservableExtensions.Subscribe(m_CollapseButton.OnLeftClickAsObservable(), delegate
			{
				OnExpandClick();
			}));
			AddDisposable(base.ViewModel.IsExpanded.ObserveLastValueOnLateUpdate().Subscribe(delegate
			{
				OnChangeSelectedState(base.ViewModel.IsSelected.Value);
			}));
		}
		else
		{
			m_CollapseButton.gameObject.SetActive(value: false);
		}
	}

	protected virtual void OnClick()
	{
		base.ViewModel.SetSelectedFromView(!base.ViewModel.IsSelected.Value);
		if (!base.ViewModel.AllowSwitchOff)
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.Value);
		}
	}

	public void OnExpandClick()
	{
		base.ViewModel.SetExpanded(!base.ViewModel.IsExpanded.Value);
	}

	public virtual void OnChangeSelectedState(bool value)
	{
		if (base.ViewModel.IsAvailable.Value)
		{
			m_Button.SetInteractable(state: true);
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
		}
		else
		{
			m_Button.SetActiveLayer("Normal");
			m_Button.SetInteractable(state: false);
		}
		if ((bool)m_CollapseButton)
		{
			if (base.ViewModel.IsAvailable.Value)
			{
				m_CollapseButton.SetInteractable(state: true);
				m_CollapseButton.SetActiveLayer(base.ViewModel.IsExpanded.Value ? "Expanded" : "Collapsed");
			}
			else
			{
				m_CollapseButton.SetActiveLayer("Collapsed");
				m_CollapseButton.SetInteractable(state: false);
			}
		}
	}

	public virtual void RefreshView()
	{
	}

	protected virtual void ClearView()
	{
	}

	protected override void DestroyViewImplementation()
	{
		m_Button.SetActiveLayer("Normal");
		ClearView();
		if ((bool)m_CollapseButton)
		{
			m_CollapseButton.SetActiveLayer("Collapsed");
		}
	}
}
