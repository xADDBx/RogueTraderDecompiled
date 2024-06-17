using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.ColonyManagement;

public class ColonyManagementNavigationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IColonizationHandler, ISubscriber
{
	public readonly AutoDisposingList<ColonyManagementNavigationElementVM> NavigationElements = new AutoDisposingList<ColonyManagementNavigationElementVM>();

	public readonly ReactiveCommand UpdateNavigationElementsCommand = new ReactiveCommand();

	private readonly Dictionary<Colony, ColonyManagementNavigationElementVM> m_ColoniesNavElements = new Dictionary<Colony, ColonyManagementNavigationElementVM>();

	private Colony m_CurrentColony;

	private List<ColoniesState.ColonyData> Colonies => Game.Instance.Player.ColoniesState.Colonies;

	public ColonyManagementNavigationVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		UpdateNavigationElements();
	}

	public void HandleColonyCreated(Colony colony, PlanetEntity planetEntity)
	{
		UpdateNavigationElements();
	}

	public void Clear()
	{
		NavigationElements.Clear();
		m_ColoniesNavElements.Clear();
	}

	public void HandleColonyPage(Colony colony)
	{
		m_CurrentColony = colony;
		SetSelection(colony);
	}

	public int GetActiveColonyIndex()
	{
		if (m_ColoniesNavElements.TryGetValue(m_CurrentColony, out var value))
		{
			return NavigationElements.IndexOf(value);
		}
		return 0;
	}

	public void SelectNextColony()
	{
		int index = Mathf.Clamp(GetActiveColonyIndex() + 1, 0, NavigationElements.Count - 1);
		NavigationElements[index].SelectPage();
	}

	public void SelectPrevColony()
	{
		int index = Mathf.Clamp(GetActiveColonyIndex() - 1, 0, NavigationElements.Count - 1);
		NavigationElements[index].SelectPage();
	}

	private void UpdateNavigationElements()
	{
		Clear();
		foreach (ColoniesState.ColonyData colony in Colonies)
		{
			ColonyManagementNavigationElementVM colonyManagementNavigationElementVM = new ColonyManagementNavigationElementVM(colony.Colony);
			AddDisposable(colonyManagementNavigationElementVM);
			NavigationElements.Add(colonyManagementNavigationElementVM);
			m_ColoniesNavElements[colony.Colony] = colonyManagementNavigationElementVM;
		}
		UpdateNavigationElementsCommand.Execute();
	}

	private void SetSelection(Colony colony)
	{
		foreach (ColonyManagementNavigationElementVM value in m_ColoniesNavElements.Values)
		{
			value.SetSelection(colony);
		}
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}
}
