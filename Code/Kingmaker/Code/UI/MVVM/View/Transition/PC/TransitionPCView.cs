using Kingmaker.Code.UI.MVVM.View.Transition.Common;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Transition.PC;

public class TransitionPCView : TransitionBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(CurrentPart.Close.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.Close();
		}));
	}
}
