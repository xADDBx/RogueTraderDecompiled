using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UI.Sound;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsButtonVM : ColonyUIComponentVM
{
	public void OpenColonyProjects()
	{
		if (m_Colony == null)
		{
			PFLog.System.Error("ColonyProjectsButtonVM.OpenColonyProjects - can't open colony projects, colony is null!");
			return;
		}
		UISounds.Instance.Sounds.SpaceColonization.ProjectWindowOpen.Play();
		Game.Instance.GameCommandQueue.ColonyProjectsUIOpen(m_Colony.Blueprint.ToReference<BlueprintColonyReference>());
	}
}
