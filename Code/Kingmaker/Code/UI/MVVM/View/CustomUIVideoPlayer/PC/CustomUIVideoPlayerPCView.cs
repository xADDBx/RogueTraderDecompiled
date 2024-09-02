using Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.Base;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.CustomUIVideoPlayer.PC;

public class CustomUIVideoPlayerPCView : CustomUIVideoPlayerBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(ObservableExtensions.Subscribe(m_PlayPauseBigButton.OnLeftClickAsObservable(), delegate
		{
			if (!VideoIsStarted)
			{
				StartVideo();
			}
			else
			{
				PlayPauseVideo();
			}
		}));
		AddDisposable(this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ShowHideInterface(state: true);
		}));
		AddDisposable(this.OnPointerExitAsObservable().Subscribe(delegate
		{
			ShowHideInterface(state: false);
		}));
	}
}
