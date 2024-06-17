using System.Collections.Generic;
using System.Linq;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectsNavigationBaseView : ViewBase<ColonyProjectsNavigationVM>
{
	[SerializeField]
	protected ColonyProjectsNavigationBlock[] m_ColonyProjectsNavigationBlocks;

	[SerializeField]
	protected ColonyProjectRankView[] m_ColonyProjectsRankItems;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	public void Initialize()
	{
		ColonyProjectRankView[] colonyProjectsRankItems = m_ColonyProjectsRankItems;
		for (int i = 0; i < colonyProjectsRankItems.Length; i++)
		{
			colonyProjectsRankItems[i].Initialize();
		}
	}

	protected override void BindViewImplementation()
	{
		for (int i = 0; i < base.ViewModel.Ranks.Count; i++)
		{
			m_ColonyProjectsRankItems[i].Bind(base.ViewModel.Ranks[i]);
		}
		DrawEntities();
		AddDisposable(base.ViewModel.ProjectsCreated.Subscribe(DrawEntities));
		AddDisposable(base.ViewModel.ProjectsUpdated.Subscribe(UpdateEntities));
		m_ScrollRect.ScrollToTop();
	}

	protected override void DestroyViewImplementation()
	{
	}

	public List<IConsoleNavigationEntity> GetNavigationEntities()
	{
		List<IConsoleNavigationEntity> list = new List<IConsoleNavigationEntity>();
		ColonyProjectsNavigationBlock[] colonyProjectsNavigationBlocks = m_ColonyProjectsNavigationBlocks;
		for (int i = 0; i < colonyProjectsNavigationBlocks.Length; i++)
		{
			ColonyProjectsNavigationBlock colonyProjectsNavigationBlock = colonyProjectsNavigationBlocks[i];
			list.AddRange(colonyProjectsNavigationBlock.WidgetList.GetNavigationEntities());
		}
		return list;
	}

	public void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	private void DrawEntities()
	{
		DrawEntitiesImpl();
		ColonyProjectRankView[] colonyProjectsRankItems = m_ColonyProjectsRankItems;
		for (int i = 0; i < colonyProjectsRankItems.Length; i++)
		{
			colonyProjectsRankItems[i].SetAsFirstSibling();
		}
		EventBus.RaiseEvent(delegate(IColonyProjectsUpdatedHandler h)
		{
			h.HandleColonyProjectsUpdated();
		});
	}

	protected virtual void DrawEntitiesImpl()
	{
	}

	private void UpdateEntities()
	{
		ColonyProjectsNavigationBlock[] colonyProjectsNavigationBlocks = m_ColonyProjectsNavigationBlocks;
		for (int i = 0; i < colonyProjectsNavigationBlocks.Length; i++)
		{
			ColonyProjectsNavigationBlock block = colonyProjectsNavigationBlocks[i];
			IEnumerable<ColonyProjectVM> source = base.ViewModel.NavigationElements.Where((ColonyProjectVM elem) => elem.Rank == block.Rank);
			block.Container.SetActive(!source.Empty() && source.Any((ColonyProjectVM entry) => entry.IsVisible));
		}
		EventBus.RaiseEvent(delegate(IColonyProjectsUpdatedHandler h)
		{
			h.HandleColonyProjectsUpdated();
		});
	}
}
