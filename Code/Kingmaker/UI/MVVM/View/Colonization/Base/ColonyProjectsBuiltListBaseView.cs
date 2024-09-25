using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public abstract class ColonyProjectsBuiltListBaseView : ViewBase<ColonyProjectsBuiltListVM>
{
	protected bool m_HasAddButton;

	protected int m_LastFreeElemIndex;

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UpdateProjectsCommand.Subscribe(UpdateColonyProjects));
		AddDisposable(base.ViewModel.ClearProjects.Subscribe(ClearProjects));
		ClearProjects();
		UpdateColonyProjects();
	}

	protected override void DestroyViewImplementation()
	{
		ClearProjects();
	}

	public List<IFloatConsoleNavigationEntity> GetNavigationEntities()
	{
		return GetNavigationEntitiesImpl();
	}

	protected abstract List<IFloatConsoleNavigationEntity> GetNavigationEntitiesImpl();

	private void UpdateColonyProjects()
	{
		UpdateColonyProjectsImpl();
	}

	protected virtual void UpdateColonyProjectsImpl()
	{
	}

	private void ClearProjects()
	{
		ClearProjectsImpl();
	}

	protected virtual void ClearProjectsImpl()
	{
	}
}
