using System;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
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
			PFLog.UI.Error("ColonyProjectsBuiltListNewElementButtonVM.OpenColonyProjects - can't open colony projects, colony is null!");
			return;
		}
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIOpen(m_Colony);
		});
	}
}
