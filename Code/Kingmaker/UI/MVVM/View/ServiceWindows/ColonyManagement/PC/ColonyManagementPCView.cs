using JetBrains.Annotations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.PC;

public class ColonyManagementPCView : ColonyManagementBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ColonyManagementNavigationPCView m_Navigation;

	[SerializeField]
	[UsedImplicitly]
	private ColonyManagementPagePCView m_Page;

	protected override void InitializeImpl()
	{
		m_Page.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_Navigation.Bind(base.ViewModel.NavigationVM);
		AddDisposable(base.ViewModel.ColonyManagementPage.Subscribe(m_Page.Bind));
		base.BindViewImplementation();
	}

	protected override void OnHideImpl()
	{
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIClose();
		});
	}

	protected override void SetLockUIForDialogImpl(bool value)
	{
		m_Navigation.SetInteractable(!value);
	}
}
