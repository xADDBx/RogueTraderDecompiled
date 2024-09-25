using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public abstract class ExplorationColonyEventsWrapperBaseView<TColonyEventsView> : ExplorationComponentWrapperBaseView<ExplorationColonyEventsWrapperVM> where TColonyEventsView : ColonyEventsBaseView
{
	[SerializeField]
	private TColonyEventsView m_ColonyEventsView;

	public void Initialize()
	{
		m_ColonyEventsView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyEventsView.Bind(base.ViewModel.ColonyEventsVM);
	}

	public void ScrollList(ColonyEventBaseView entity)
	{
		m_ColonyEventsView.ScrollList(entity);
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		if (base.ViewModel.ActiveOnScreen.Value)
		{
			return m_ColonyEventsView.GetNavigationEntities();
		}
		return null;
	}
}
