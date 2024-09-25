using Kingmaker.Settings;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public abstract class TooltipBaseBrickView<T> : ViewBase<T> where T : TooltipBaseBrickVM
{
	protected float FontMultiplier;

	protected override void BindViewImplementation()
	{
		FontMultiplier = SettingsRoot.Accessiability.FontSizeMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
	}
}
