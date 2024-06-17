using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoRankEntryPCView : CharInfoFeatureSimpleBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetTooltip();
	}

	private void SetTooltip()
	{
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
		}
	}
}
