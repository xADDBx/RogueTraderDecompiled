using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;

public class AllSystemsInformationWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IGlobalMapSpaceSystemInformationWindowHandler, ISubscriber, IGlobalMapAllSystemsInformationWindowHandler, IGlobalMapInformationWindowsConsoleHandler
{
	public readonly ReactiveProperty<bool> ShowAllSystemsWindow = new ReactiveProperty<bool>();

	public readonly List<SystemInfoAllSystemsInformationWindowVM> Systems = new List<SystemInfoAllSystemsInformationWindowVM>();

	public readonly ReactiveProperty<bool> IsAllSystemsOnTheFront = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<SectorMapObjectEntity> SectorMapObjectEntity = new ReactiveProperty<SectorMapObjectEntity>();

	public AllSystemsInformationWindowVM()
	{
		ShowAllSystemsWindow.Value = false;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void CloseWindow()
	{
		ShowAllSystemsWindow.Value = false;
		SectorMapOvertipsVM.Instance.BlockPopups(state: false);
		EventBus.RaiseEvent(delegate(IGlobalMapSetAllSystemsInformationWindowStateHandler h)
		{
			h.HandleSetAllSystemsInformationWindowState(state: false);
		});
	}

	public void HandleShowSpaceSystemInformationWindow()
	{
		CloseWindow();
	}

	public void HandleHideSpaceSystemInformationWindow()
	{
	}

	public void HandleShowAllSystemsInformationWindow()
	{
		SetSystemSettings();
		ShowAllSystemsWindow.Value = true;
	}

	public void HandleHideAllSystemsInformationWindow()
	{
		CloseWindow();
	}

	private void SetSystemSettings()
	{
		AddSystemsInfo();
	}

	private void AddSystemsInfo()
	{
		Systems.Clear();
		List<SectorMapObjectEntity> list = (from entity in Game.Instance.State.SectorMapObjects
			where entity.View.IsSystem
			where entity.IsVisited
			select entity).ToList();
		if (list.Any())
		{
			list.ForEach(delegate(SectorMapObjectEntity system)
			{
				Systems.Add(new SystemInfoAllSystemsInformationWindowVM(system));
			});
			Systems.Sort((SystemInfoAllSystemsInformationWindowVM p, SystemInfoAllSystemsInformationWindowVM q) => string.Compare(p.SystemName, q.SystemName, StringComparison.Ordinal));
		}
	}

	public void HandleShowSystemInformationWindowConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
	}

	public void HandleHideSystemInformationWindowConsole()
	{
	}

	public void HandleShowAllSystemsInformationWindowConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
		SetSystemSettings();
		SectorMapObjectEntity.Value = sectorMapObjectEntity ?? Game.Instance.SectorMapController.CurrentStarSystem;
		ShowAllSystemsWindow.Value = true;
	}

	public void HandleHideAllSystemsInformationWindowConsole()
	{
		CloseWindow();
	}

	public void HandleChangeInformationWindowsConsole()
	{
		IsAllSystemsOnTheFront.Value = !IsAllSystemsOnTheFront.Value;
	}

	public void HandleChangeCurrentSystemInfoConsole(SectorMapObjectEntity sectorMapObjectEntity)
	{
		SectorMapObjectEntity.Value = sectorMapObjectEntity ?? Game.Instance.SectorMapController.CurrentStarSystem;
	}
}
