using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectsBuiltListPCView : ColonyProjectsBuiltListBaseView
{
	[SerializeField]
	private ColonyProjectsBuiltListAddElemPCView m_ColonyProjectsBuiltListAddElemPCView;

	[SerializeField]
	private List<ColonyProjectsBuiltListElemPCView> m_ColonyProjects;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyProjectsBuiltListAddElemPCView.Bind(base.ViewModel.ColonyProjectsBuiltListAddElemVM);
	}

	protected override List<IFloatConsoleNavigationEntity> GetNavigationEntitiesImpl()
	{
		List<IFloatConsoleNavigationEntity> list = new List<IFloatConsoleNavigationEntity>();
		if (m_HasAddButton)
		{
			list.AddRange(m_ColonyProjects.Except(m_ColonyProjects[m_LastFreeElemIndex]));
			list.Add(m_ColonyProjectsBuiltListAddElemPCView);
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
				PFLog.UI.Error("ColonyProjectsBuiltListPCView.UpdateColonyProjectsImpl - ProjectsVMs count is more than slots count!");
				return;
			}
			ColonyProjectsBuiltListElemPCView colonyProjectsBuiltListElemPCView = m_ColonyProjects[i];
			colonyProjectsBuiltListElemPCView.Bind(base.ViewModel.ProjectsVMs[i]);
			colonyProjectsBuiltListElemPCView.SetBindedVisual();
		}
		int count = base.ViewModel.ProjectsVMs.Count;
		if (count >= m_ColonyProjects.Count)
		{
			m_ColonyProjectsBuiltListAddElemPCView.gameObject.SetActive(value: false);
			return;
		}
		m_ColonyProjectsBuiltListAddElemPCView.gameObject.SetActive(value: true);
		m_ColonyProjectsBuiltListAddElemPCView.transform.localPosition = m_ColonyProjects[count].transform.localPosition;
	}

	protected override void ClearProjectsImpl()
	{
		foreach (ColonyProjectsBuiltListElemPCView colonyProject in m_ColonyProjects)
		{
			colonyProject.Unbind();
			colonyProject.SetUnbindedVisual();
		}
	}
}
