using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Menu;

public class ServiceWindowsMenuVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly SelectionGroupRadioVM<ServiceWindowsMenuEntityVM> SelectionGroup;

	private List<ServiceWindowsMenuEntityVM> m_EntitiesList;

	private readonly Action<ServiceWindowsType> m_OnSelect;

	public readonly ReactiveProperty<bool> IsAdditionalBackgroundNeeded = new ReactiveProperty<bool>();

	public ReactiveProperty<ServiceWindowsMenuEntityVM> SelectedEntity { get; }

	public ServiceWindowsMenuVM(Action<ServiceWindowsType> onSelect)
	{
		m_OnSelect = onSelect;
		CreateEntities();
		SelectedEntity = new ReactiveProperty<ServiceWindowsMenuEntityVM>();
		AddDisposable(SelectionGroup = new SelectionGroupRadioVM<ServiceWindowsMenuEntityVM>(m_EntitiesList, SelectedEntity));
		AddDisposable(SelectedEntity.Skip(1).Subscribe(OnEntitySelected));
	}

	protected override void DisposeImplementation()
	{
	}

	public void SelectWindow(ServiceWindowsType type)
	{
		ReactiveProperty<bool> isAdditionalBackgroundNeeded = IsAdditionalBackgroundNeeded;
		ServiceWindowsType currentServiceWindow = Game.Instance.RootUiContext.CurrentServiceWindow;
		isAdditionalBackgroundNeeded.Value = currentServiceWindow == ServiceWindowsType.Encyclopedia || currentServiceWindow == ServiceWindowsType.Journal || currentServiceWindow == ServiceWindowsType.LocalMap;
		SelectedEntity.SetValueAndForceNotify(m_EntitiesList.FirstOrDefault((ServiceWindowsMenuEntityVM e) => e.ServiceWindowsType == type));
	}

	private void CreateEntities()
	{
		m_EntitiesList = new List<ServiceWindowsMenuEntityVM>();
		foreach (ServiceWindowsType value in Enum.GetValues(typeof(ServiceWindowsType)))
		{
			if (value != 0)
			{
				ServiceWindowsMenuEntityVM serviceWindowsMenuEntityVM = new ServiceWindowsMenuEntityVM(value);
				AddDisposable(serviceWindowsMenuEntityVM);
				m_EntitiesList.Add(serviceWindowsMenuEntityVM);
			}
		}
	}

	private void OnEntitySelected(ServiceWindowsMenuEntityVM entity)
	{
		m_OnSelect?.Invoke(entity?.ServiceWindowsType ?? ServiceWindowsType.None);
	}

	public void Close()
	{
		if (!Game.Instance.RootUiContext.ServiceWindowNowIsOpening)
		{
			m_OnSelect?.Invoke(ServiceWindowsType.None);
		}
	}
}
