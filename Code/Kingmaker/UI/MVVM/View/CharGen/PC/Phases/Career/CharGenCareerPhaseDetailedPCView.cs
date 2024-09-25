using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Career;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using UniRx;

namespace Kingmaker.UI.MVVM.View.CharGen.PC.Phases.Career;

public class CharGenCareerPhaseDetailedPCView : CharGenCareerPhaseDetailedView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.UnitProgressionVM.State.Subscribe(UpdateInfo));
	}

	private void UpdateInfo(UnitProgressionWindowState state)
	{
		if (state == UnitProgressionWindowState.CareerPathProgression)
		{
			base.ViewModel.InfoVM.SetTemplate(null);
		}
		else
		{
			base.ViewModel.UpdateTooltipTemplate(base.ViewModel.UnitProgressionVM?.PreselectedCareer.Value?.CareerPath);
		}
	}
}
