using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public class CharInfoComponentWithLevelUpView<TViewModel> : CharInfoComponentView<TViewModel> where TViewModel : CharInfoComponentWithLevelUpVM
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.PreviewUnit.Subscribe(delegate
		{
			RefreshView();
		}));
	}
}
