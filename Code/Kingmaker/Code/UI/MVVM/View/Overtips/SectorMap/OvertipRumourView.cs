using Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.SectorMap;

public abstract class OvertipRumourView : BaseOvertipView<OvertipEntityRumourVM>
{
	protected override bool CheckVisibility => base.ViewModel.IsVisible.Value;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		base.name = base.ViewModel.SectorMapRumour.View.name + "_OvertipRumourView";
	}
}
