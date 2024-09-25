using Kingmaker.UI.InputSystems;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction;

public class VariativeInteractionPCView : VariativeInteractionView<InteractionVariantPCView>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
	}
}
