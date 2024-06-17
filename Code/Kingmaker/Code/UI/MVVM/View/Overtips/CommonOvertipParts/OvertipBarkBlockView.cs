using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.UI.MVVM.View.Bark.PC;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;

public class OvertipBarkBlockView : BarkBlockView<OvertipBarkBlockVM>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsBarkActive.Subscribe(delegate(bool value)
		{
			FadeAnimator.PlayAnimation(value);
		}));
	}
}
