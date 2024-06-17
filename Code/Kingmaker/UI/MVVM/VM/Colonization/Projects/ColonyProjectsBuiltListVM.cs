using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsBuiltListVM : ColonyUIComponentVM, IColonizationHandler, ISubscriber, IColonizationProjectsHandler
{
	public readonly AutoDisposingReactiveCollection<ColonyProjectVM> ProjectsVMs = new AutoDisposingReactiveCollection<ColonyProjectVM>();

	public readonly ReactiveCommand UpdateProjectsCommand = new ReactiveCommand();

	public readonly ReactiveCommand ClearProjects = new ReactiveCommand();

	public readonly ColonyProjectsBuiltListAddElemVM ColonyProjectsBuiltListAddElemVM;

	public ColonyProjectsBuiltListVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(ColonyProjectsBuiltListAddElemVM = new ColonyProjectsBuiltListAddElemVM());
	}

	protected override void DisposeImplementation()
	{
		ProjectsVMs.Clear();
	}

	protected override void SetColonyImpl(Colony colony)
	{
		ColonyProjectsBuiltListAddElemVM.SetColony(colony);
		SetProjects();
	}

	public void HandleColonyCreated(Colony colony, PlanetEntity planetEntity)
	{
		SetProjects();
	}

	public void HandleColonyProjectStarted(Colony colony, ColonyProject project)
	{
		SetProjects();
	}

	public void HandleColonyProjectFinished(Colony colony, ColonyProject project)
	{
		SetProjects();
	}

	private void SetProjects()
	{
		if (m_Colony != null)
		{
			ProjectsVMs.Clear();
			ClearProjects.Execute();
			AddProjects();
		}
	}

	private void AddProjects()
	{
		if (m_Colony == null)
		{
			return;
		}
		foreach (ColonyProject project in m_Colony.Projects)
		{
			ColonyProjectVM colonyProjectVM = new ColonyProjectVM(project.Blueprint, m_Colony);
			colonyProjectVM.FullUpdate();
			AddDisposable(colonyProjectVM);
			ProjectsVMs.Add(colonyProjectVM);
		}
		UpdateProjectsCommand.Execute();
	}
}
