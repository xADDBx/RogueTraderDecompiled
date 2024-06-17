using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.VariativeInteraction;

public class InteractionVariantPCView : InteractionVariantView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnSingleLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Interact();
		}));
		AddDisposable(m_Button.OnSingleLeftClickNotInteractableAsObservable().Subscribe(delegate
		{
			base.ViewModel.Interact();
		}));
	}
}
