using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsButtonVM : ColonyUIComponentVM
{
	public void OpenColonyProjects()
	{
		if (m_Colony == null)
		{
			PFLog.UI.Error("ColonyProjectsButtonVM.OpenColonyProjects - can't open colony projects, colony is null!");
			return;
		}
		UISounds.Instance.Sounds.SpaceColonization.ProjectWindowOpen.Play();
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler h)
		{
			h.HandleColonyProjectsUIOpen(m_Colony);
		});
	}
}
