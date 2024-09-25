using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyProjectsBuiltListConsoleView : ColonyProjectsBuiltListBaseView
{
	[SerializeField]
	private ColonyProjectsBuiltListAddElemConsoleView m_ColonyProjectsBuiltListAddElemConsoleView;

	[SerializeField]
	private List<ColonyProjectsBuiltListElemConsoleView> m_ColonyProjects;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyProjectsBuiltListAddElemConsoleView.Bind(base.ViewModel.ColonyProjectsBuiltListAddElemVM);
	}

	protected override List<IFloatConsoleNavigationEntity> GetNavigationEntitiesImpl()
	{
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		if (m_HasAddButton)
		{
			list.AddRange(m_ColonyProjects.Except(m_ColonyProjects[m_LastFreeElemIndex]));
			list.Add(m_ColonyProjectsBuiltListAddElemConsoleView);
		}
		else
		{
			list.AddRange(m_ColonyProjects);
		}
		return list;
	}

	protected override void UpdateColonyProjectsImpl()
	{
		for (int i = 0; i < base.ViewModel.ProjectsVMs.Count; i++)
		{
			if (i >= m_ColonyProjects.Count)
			{
				PFLog.UI.Error("ColonyProjectsBuiltListConsoleView.UpdateColonyProjectsImpl - ProjectsVMs count is more than slots count!");
				return;
			}
			ColonyProjectsBuiltListElemConsoleView colonyProjectsBuiltListElemConsoleView = m_ColonyProjects[i];
			colonyProjectsBuiltListElemConsoleView.Bind(base.ViewModel.ProjectsVMs[i]);
			colonyProjectsBuiltListElemConsoleView.SetBindedVisual();
		}
		m_LastFreeElemIndex = base.ViewModel.ProjectsVMs.Count;
		if (m_LastFreeElemIndex >= m_ColonyProjects.Count)
		{
			m_ColonyProjectsBuiltListAddElemConsoleView.gameObject.SetActive(value: false);
			m_HasAddButton = false;
		}
		else
		{
			m_ColonyProjectsBuiltListAddElemConsoleView.gameObject.SetActive(value: true);
			m_ColonyProjectsBuiltListAddElemConsoleView.transform.localPosition = m_ColonyProjects[m_LastFreeElemIndex].transform.localPosition;
			m_HasAddButton = true;
		}
	}

	protected override void ClearProjectsImpl()
	{
		foreach (ColonyProjectsBuiltListElemConsoleView colonyProject in m_ColonyProjects)
		{
			colonyProject.Unbind();
			colonyProject.SetUnbindedVisual();
		}
	}
}
