using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationColonyTraitsWrapperBaseView<TColonyTraitsView> : ExplorationComponentWrapperBaseView<ExplorationColonyTraitsWrapperVM> where TColonyTraitsView : ColonyTraitsBaseView
{
	[SerializeField]
	private TColonyTraitsView m_ColonyTraitsView;

	public void Initialize()
	{
		m_ColonyTraitsView.Initialize();
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		if (base.ViewModel.ActiveOnScreen.Value)
		{
			return m_ColonyTraitsView.GetNavigationEntities();
		}
		return null;
	}

	public void ScrollList(ColonyTraitBaseView entity)
	{
		m_ColonyTraitsView.ScrollList(entity);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyTraitsView.Bind(base.ViewModel.ColonyTraitsVM);
	}
}
