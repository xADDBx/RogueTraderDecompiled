using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectsNavigationPCView : ColonyProjectsNavigationBaseView
{
	[SerializeField]
	private ColonyProjectPCView m_ColonyProjectViewPrefab;

	protected override void DrawEntitiesImpl()
	{
		ColonyProjectsNavigationBlock[] colonyProjectsNavigationBlocks = m_ColonyProjectsNavigationBlocks;
		for (int i = 0; i < colonyProjectsNavigationBlocks.Length; i++)
		{
			ColonyProjectsNavigationBlock block = colonyProjectsNavigationBlocks[i];
			IEnumerable<ColonyProjectVM> vmCollection = base.ViewModel.NavigationElements.Where((ColonyProjectVM elem) => elem.Rank == block.Rank);
			block.WidgetList.DrawEntries(vmCollection, m_ColonyProjectViewPrefab);
		}
	}
}
