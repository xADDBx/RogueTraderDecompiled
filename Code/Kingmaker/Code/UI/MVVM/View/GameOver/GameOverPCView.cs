using Kingmaker.UI.InputSystems;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public class GameOverPCView : GameOverView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
		}));
	}
}
