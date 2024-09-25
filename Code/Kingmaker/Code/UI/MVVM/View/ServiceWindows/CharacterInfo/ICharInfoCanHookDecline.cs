using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public interface ICharInfoCanHookDecline
{
	IReadOnlyReactiveProperty<bool> GetCanHookDeclineProperty();
}
