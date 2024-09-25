using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UI.SelectionGroup.View;

public class SelectionGroupEntityView<TViewModel> : VirtualListElementViewBase<TViewModel>, IConfirmClickHandler, IConsoleEntity, IConsolePointerLeftClickEvent, IConsoleNavigationEntity where TViewModel : SelectionGroupEntityVM
{
	private bool m_IsInit;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	public ReactiveCommand PointerLeftClickCommand { get; } = new ReactiveCommand();


	public IConsoleEntity ConsoleEntityProxy => m_Button;

	public void ViewBind(IViewModel viewModel)
	{
		BindViewImplementation();
	}

	public void DestroyViewItem()
	{
		DestroyViewImplementation();
	}

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

	protected override void BindViewImplementation()
	{
		AddDisposable(ObservableExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			PointerLeftClickCommand.Execute();
			OnClick();
		}));
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(OnClick));
		AddDisposable(base.ViewModel.RefreshView.Subscribe(RefreshView));
		RefreshView();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(OnChangeSelectedState));
		AddDisposable(base.ViewModel.IsAvailable.Subscribe(delegate
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.Value);
		}));
	}

	protected virtual void OnClick()
	{
		base.ViewModel.SetSelectedFromView(!base.ViewModel.IsSelected.Value);
		if (base.ViewModel != null && !base.ViewModel.AllowSwitchOff)
		{
			OnChangeSelectedState(base.ViewModel.IsSelected.Value);
		}
	}

	public virtual void OnChangeSelectedState(bool value)
	{
		if (base.ViewModel.IsAvailable.Value)
		{
			m_Button.SetInteractable(state: true);
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
			m_Button.CanConfirm = !value || base.ViewModel.AllowSwitchOff;
		}
		else
		{
			m_Button.SetInteractable(state: false);
			m_Button.SetActiveLayer(value ? "Selected" : "Normal");
			m_Button.CanConfirm = !value;
		}
	}

	public virtual void RefreshView()
	{
	}

	protected override void DestroyViewImplementation()
	{
		m_Button.SetActiveLayer("Normal");
		m_Button.CanConfirm = true;
		ClearView();
	}

	protected virtual void ClearView()
	{
	}

	public virtual void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public virtual bool IsValid()
	{
		return m_Button.Interactable;
	}

	public bool CanConfirmClick()
	{
		return m_Button.CanConfirm;
	}

	public string GetConfirmClickHint()
	{
		return GetConfirmActionName();
	}

	protected virtual string GetConfirmActionName()
	{
		return string.Empty;
	}

	public void OnConfirmClick()
	{
		m_Button.OnConfirmClick();
	}
}
