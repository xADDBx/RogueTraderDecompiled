using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;
using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia;

public class EncyclopediaFullScreenPageImagePCView : EncyclopediaFullScreenPageImageBaseView
{
	protected override void BindViewImplementation()
	{
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			Close();
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			Close();
		}));
		base.BindViewImplementation();
	}
}
