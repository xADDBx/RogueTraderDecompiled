using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia;

public class EncyclopediaPageImagePCView : EncyclopediaPageImageBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ZoomButton.gameObject.SetActive(base.ViewModel.IsZoomAllowed);
		if (base.ViewModel.IsZoomAllowed)
		{
			AddDisposable(m_ZoomButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				OnButtonClick();
			}));
		}
	}
}
