using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public interface ICharInfoCanHookConfirm
{
	IReadOnlyReactiveProperty<bool> GetCanHookConfirmProperty();
}
