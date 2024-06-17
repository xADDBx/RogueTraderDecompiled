using System;
using Kingmaker.Blueprints;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsBuiltListAddElemVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private Colony m_Colony;

	protected override void DisposeImplementation()
	{
		m_Colony = null;
	}

	public void SetColony(Colony colony)
	{
		m_Colony = colony;
	}

	public void OpenColonyProjects()
	{
		if (m_Colony == null)
		{
			PFLog.System.Error("ColonyProjectsBuiltListNewElementButtonVM.OpenColonyProjects - can't open colony projects, colony is null!");
		}
		else
		{
			Game.Instance.GameCommandQueue.ColonyProjectsUIOpen(m_Colony.Blueprint.ToReference<BlueprintColonyReference>());
		}
	}
}
